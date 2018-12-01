// -----------------------------------------------------------------------
// <copyright file="JsonPlusParser.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Defines a callback function for returning data using the `include` directive.
    /// </summary>
    /// <param name="callbackType">The type of include source, such as file, URL, or resources embedded in assemblies.</param>
    /// <param name="value">The resource path.</param>
    /// <returns>An asynchronous task that contains the data returned by the `include` directive.</returns>
    public delegate Task<string> IncludeCallbackAsync(IncludeSource callbackType, string value);

    /// <summary>
    /// This class contains methods used to parse Json+ source code.
    /// </summary>
    public sealed partial class JsonPlusParser
    {
        private readonly List<JsonPlusSubstitution> _substitutions = new List<JsonPlusSubstitution>();
        private IncludeCallbackAsync _includeCallback = (type, value) => Task.FromResult("{}");

        private TokenizeResult _tokens;
        private JsonPlusValue _root;

        private JsonPlusPath Path { get; } = new JsonPlusPath();

        /// <summary>
        /// Parses the Json+ source code specified into structured objects.
        /// </summary>
        /// <param name="source">The source code that conforms to Json+ specification.</param>
        /// <param name="includeCallback">Callback used to resolve the `include` directive.</param>
        /// <param name="resolveEnv">Allow substitutions to access environment variables. Defaults to `false`.</param>
        /// <exception cref="JsonPlusParserException">An unresolved substitution has occured, or an error occured at the tokenizing or parsing stage.</exception>
        /// <returns>The root node from parsing <paramref name="source"/> and any included resources.</returns>
        public static JsonPlusRoot Parse(string source, IncludeCallbackAsync includeCallback = null, bool resolveEnv = false)
        {
            return new JsonPlusParser().ParseSource(source, true, resolveEnv, includeCallback);
        }

        /// <summary>
        /// Parses the Json+ source code specified into structured objects.
        /// </summary>
        /// <param name="source">The source code that conforms to Json+ specification.</param>
        /// <param name="includeCallback">Callback used to resolve the `include` directive.</param>
        /// <param name="resolveSubstitutions">Resolve substitution directives.</param>
        /// <param name="resolveEnv">Try to resolve environment variables. Does nothing if <paramref name="resolveSubstitutions"/> is `false`.</param>
        /// <returns></returns>
        private JsonPlusRoot ParseSource(string source, bool resolveSubstitutions, bool resolveEnv, IncludeCallbackAsync includeCallback)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new JsonPlusParserException(string.Format(RS.SourceEmptyError, nameof(source)));

            if (includeCallback != null)
                _includeCallback = includeCallback;

            try
            {
                _tokens = new JPlusTokenizer(source).Tokenize();
                _root = new JsonPlusValue(null);
                ParseTokens();
                if (resolveSubstitutions)
                    ResolveAllSubstitution(resolveEnv);
            }
            catch (JsonPlusTokenizerException e)
            {
                throw JsonPlusParserException.Create(e, null, string.Format(RS.TokenizeError, e.Message), e);
            }
            catch (JsonPlusException e)
            {
                throw JsonPlusParserException.Create(_tokens.Current, Path, e.Message, e);
            }

            return new JsonPlusRoot(_root, _substitutions);
        }

        private void ResolveAllSubstitution(bool resolveEnv)
        {
            foreach (JsonPlusSubstitution sub in _substitutions)
            {
                // Retrieve value
                JsonPlusValue res;
                try
                {
                    res = ResolveSubstitution(sub);
                }
                catch(JsonPlusException e)
                {
                    throw JsonPlusParserException.Create(sub, sub.Path, string.Format(RS.SubstitutionError, e.Message), e);
                }

                if (res != null)
                {
                    sub.ResolvedValue = res;
                    continue;
                }

                if (resolveEnv)
                {
                    // Try to pull value from environment
                    string envValue = null;
                    try
                    {
                        envValue = Environment.GetEnvironmentVariable(sub.Path.Value);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (envValue != null)
                    {
                        // undefined value resolved to an environment variable
                        res = new JsonPlusValue(sub.Parent);
                        if (envValue.NeedQuotes())
                            res.Add(new QuotedStringValue(sub.Parent, envValue));
                        else
                            res.Add(new UnquotedStringValue(sub.Parent, envValue));

                        sub.ResolvedValue = res;
                        continue;
                    }
                }

                // ${ throws exception if it is not resolved
                if (sub.Required)
                    throw JsonPlusParserException.Create(sub, sub.Path, string.Format(RS.UnresolvedSubstitution, sub.Path));

                sub.ResolvedValue = new EmptyValue(sub.Parent);
            }
        }

        private JsonPlusValue ResolveSubstitution(JsonPlusSubstitution sub)
        {
            JsonPlusObjectMember subField = sub.ParentMember;

            // first case, this substitution is a direct self-reference
            if (sub.Path == sub.GetMemberPath())
            {
                IJsonPlusNode parent = sub.Parent;
                while (parent is JsonPlusValue)
                {
                    parent = parent.Parent;
                }

                // Fail case
                if (parent is JsonPlusArray)
                    throw new JsonPlusException(RS.SelfRefSubstitutionInArray);

                // try to resolve substitution by looking backward in the field assignment stack
                return subField.OlderValueThan(sub);
            }

            // need to recursively get full path
            JsonPlusPath fieldPath = subField.GetMemberPath();

            // second case, the substitution references a field child in the past
            if (sub.Path.IsChildPathOf(fieldPath))
            {
                JsonPlusValue olderValue = subField.OlderValueThan(sub);
                if ((olderValue != null) && 
                    (olderValue.Type == JsonPlusType.Object))
                {
                    int difLength = sub.Path.Count - fieldPath.Count;
                    JsonPlusPath deltaPath = sub.Path.SubPath(sub.Path.Count - difLength, difLength);

                    JsonPlusObject olderObject = olderValue.GetObject();
                    if (olderObject.TryGetValue(deltaPath, out JsonPlusValue innerValue))
                        return innerValue.Type == JsonPlusType.Object ? innerValue : null;
                }
            }

            // Detect invalid parent-referencing substitution
            if (fieldPath.IsChildPathOf(sub.Path))
                throw new JsonPlusException(RS.SubstitutionRefDirectParentError);

            // Detect invalid cyclic reference loop
            if (IsValueCyclic(subField, sub))
                throw new JsonPlusException(RS.CyclicSubstitutionLoop);

            // third case, regular substitution
            _root.GetObject().TryGetValue(sub.Path, out JsonPlusValue field);
            return field;
        }

        private bool IsValueCyclic(JsonPlusObjectMember field, JsonPlusSubstitution sub)
        {
            Stack<JsonPlusValue> pendingValues = new Stack<JsonPlusValue>();
            List<JsonPlusObjectMember> visitedFields = new List<JsonPlusObjectMember> { field };
            Stack<JsonPlusSubstitution> pendingSubs = new Stack<JsonPlusSubstitution>();
            pendingSubs.Push(sub);

            while (pendingSubs.Count > 0)
            {
                JsonPlusSubstitution currentSub = pendingSubs.Pop();
                if (!_root.GetObject().TryGetMember(currentSub.Path, out var currentField))
                    continue;

                if (visitedFields.Contains(currentField))
                    return true;

                visitedFields.Add(currentField);
                pendingValues.Push(currentField.Value);
                while (pendingValues.Count > 0)
                {
                    JsonPlusValue currentValue = pendingValues.Pop();

                    foreach (IJsonPlusNode value in currentValue)
                    {
                        switch (value)
                        {
                            case JsonPlusLiteralValue _:
                                break;

                            case JsonPlusObject o:
                                foreach (JsonPlusObjectMember f in o.Values)
                                {
                                    if (visitedFields.Contains(f))
                                        return true;

                                    visitedFields.Add(f);
                                    pendingValues.Push(f.Value);
                                }
                                break;

                            case JsonPlusArray a:
                                foreach (JsonPlusValue item in a.GetArray())
                                {
                                    pendingValues.Push(item);
                                }
                                break;

                            case JsonPlusSubstitution s:
                                pendingSubs.Push(s);
                                break;
                        }
                    }
                }
            }
            return false;
        }

        private void ParseTokens()
        {
            if (_tokens.Current.IsNonSignificant())
                ConsumeWhitelines();

            while (_tokens.Current.Type != TokenType.EndOfFile)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        _root.Add(ParseInclude(_root));
                        break;

                    // may contain one array and one array only
                    case TokenType.StartOfArray:
                        _root.Add(ParseArray(_root));
                        break;

                    case TokenType.StartOfObject:
                        _root.Add(ParseObject(_root).GetObject());
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            ConsumeWhitelines();
                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        _root.Add(ParseObject(_root).GetObject());
                        break;

                    case TokenType.Comment:
                    case TokenType.EndOfLine:
                    case TokenType.EndOfFile:
                    case TokenType.EndOfObject:
                    case TokenType.EndOfArray:
                        _tokens.Next();
                        break;

                    default:
                        throw JsonPlusParserException.Create(_tokens.Current, null, string.Format(RS.IllegalTokenType, _tokens.Current.Type), null);
                }
            }
        }

        private IJsonPlusNode ParseInclude(IJsonPlusNode owner)
        {
            // Sanity check
            if (_tokens.Current.Type != TokenType.Include)
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.InvalidTokenOnParseInclude, _tokens.Current.Type));

            int parenthesisCount = 0;
            bool required = false;
            IncludeSource callbackType = IncludeSource.Unspecified;
            string fileName = null;
            Token includeToken = _tokens.Current;

            List<TokenType> expectedTokens = new List<TokenType>(new[]
            {
                TokenType.Required,
                TokenType.Url,
                TokenType.File,
                TokenType.ClassPath,
                TokenType.LiteralValue,
                TokenType.CloseBracket,
                TokenType.EndOfLine
            });

            bool parsing = true;
            while (parsing)
            {
                if (!_tokens.GetNextSignificant(expectedTokens.ToArray()))
                    throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.InvalidTokenInInclude, _tokens.Current.Type), null);

                switch (_tokens.Current.Type)
                {
                    case TokenType.CloseBracket:
                        if (parenthesisCount == 0)
                            throw JsonPlusParserException.Create(_tokens.Current, Path, RS.UnexpectedCloseBracket, null);

                        parenthesisCount--;
                        parsing = parenthesisCount > 0;
                        break;

                    case TokenType.Required:
                        if (!_tokens.GetNextSignificant(TokenType.OpenBracket))
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedToken, TokenType.OpenBracket, _tokens.Current.Type));

                        parenthesisCount++;
                        required = true;
                        expectedTokens.Remove(TokenType.Required);
                        break;

                    case TokenType.Url:
                        if (!_tokens.GetNextSignificant(TokenType.OpenBracket))
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedToken, TokenType.OpenBracket, _tokens.Current.Type));

                        parenthesisCount++;
                        callbackType = IncludeSource.Url;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.ClassPath);
                        break;

                    case TokenType.File:
                        if (!_tokens.GetNextSignificant(TokenType.OpenBracket))
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedToken, TokenType.OpenBracket, _tokens.Current.Type));

                        parenthesisCount++;
                        callbackType = IncludeSource.File;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.ClassPath);
                        break;

                    case TokenType.ClassPath:
                        if (!_tokens.GetNextSignificant(TokenType.OpenBracket))
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedToken, TokenType.OpenBracket, _tokens.Current.Type));

                        parenthesisCount++;
                        callbackType = IncludeSource.Resource;
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.ClassPath);
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            ConsumeWhitespace();
                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (_tokens.Current.LiteralType != LiteralTokenType.QuotedLiteralValue)
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.BadIncludeFileName, LiteralTokenType.QuotedLiteralValue, _tokens.Current.LiteralType));

                        fileName = _tokens.Current.Value;
                        expectedTokens.Remove(TokenType.LiteralValue);
                        expectedTokens.Remove(TokenType.Required);
                        expectedTokens.Remove(TokenType.Url);
                        expectedTokens.Remove(TokenType.File);
                        expectedTokens.Remove(TokenType.ClassPath);

                        parsing = parenthesisCount > 0;
                        break;

                    default:
                        throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.ErrAtUnexpectedToken, _tokens.Current.Type));
                }
            }

            if (parenthesisCount > 0)
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedToken, TokenType.CloseBracket, _tokens.Current.Type));
            
            if (fileName == null)
                throw JsonPlusParserException.Create(_tokens.Current, Path, RS.FileNameMissingInInclude);

            // Consume the last token
            _tokens.Next();

            string includeSrc = _includeCallback(callbackType, fileName).ConfigureAwait(false).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(includeSrc))
            {
                if (required)
                    throw JsonPlusParserException.Create(includeToken, Path, RS.IncludeReturnEmptyError);

                return new EmptyValue(owner);
            }

            JsonPlusRoot includeRoot = new JsonPlusParser().ParseSource(includeSrc, false, false, _includeCallback);
            if (owner != null && 
                owner.Type != JsonPlusType.Empty && 
                owner.Type != includeRoot.Value.Type)
            {
                throw JsonPlusParserException.Create(includeToken, Path, string.Format(RS.IncludeMergeTypeMismatch, owner.Type, includeRoot.Value.Type));
            }

            // fixup the substitution, add the current path as a prefix to the substitution path
            foreach (JsonPlusSubstitution substitution in includeRoot.Substitutions)
            {
                substitution.Path.InsertRange(0, Path);
            }
            _substitutions.AddRange(includeRoot.Substitutions);

            // reparent the value returned by the callback to the owner of the include declaration
            return includeRoot.Value.Clone(owner);
        }

        // The owner in this context can be either an object or an array.
        private JsonPlusObject ParseObject(IJsonPlusNode owner)
        {
            JsonPlusObject jpObject = new JsonPlusObject(owner);

            if (_tokens.Current.Type != TokenType.StartOfObject &&
                _tokens.Current.Type != TokenType.LiteralValue &&
                _tokens.Current.Type != TokenType.Include)
            {
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenWith2AltInObject,
                    TokenType.StartOfObject, TokenType.LiteralValue,
                    _tokens.Current.Type));
            }

            bool headless = true;
            if (_tokens.Current.Type == TokenType.StartOfObject)
            {
                headless = false;
                ConsumeWhitelines();
            }

            IJsonPlusNode lastValue = null;
            bool parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenWith2AltInObject,
                                TokenType.ArraySeparator, TokenType.EndOfLine,
                                _tokens.Current.Type));
                        }

                        lastValue = ParseInclude(jpObject);
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            ConsumeWhitespace();
                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenWith2AltInObject,
                                TokenType.ArraySeparator, TokenType.EndOfLine,
                                _tokens.Current.Type));
                        }

                        lastValue = ParseObjectMember(jpObject);
                        break;

                    // TODO: can an object be declared floating without being assigned to a field?
                    //case TokenType.StartOfObject:
                    case TokenType.Comment:
                    case TokenType.EndOfLine:
                        switch (lastValue)
                        {
                            case null:
                                ConsumeWhitelines();
                                break;
                            case JsonPlusObjectMember _:
                                break;
                            default:
                                ((JsonPlusValue)jpObject.Parent).Add(lastValue.GetObject());
                                break;
                        }
                        lastValue = null;
                        ConsumeWhitelines();
                        break;

                    case TokenType.ArraySeparator:
                        switch (lastValue)
                        {
                            case null:
                                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenWith2AltInObject,
                                    TokenType.Assignment, TokenType.StartOfObject,
                                    _tokens.Current.Type));
                            case JsonPlusObjectMember _:
                                break;
                            default:
                                ((JsonPlusValue)jpObject.Parent).Add(lastValue.GetObject());
                                break;
                        }
                        lastValue = null;
                        ConsumeWhitelines();
                        break;

                    case TokenType.EndOfObject:
                    case TokenType.EndOfFile:
                        if (headless && _tokens.Current.Type != TokenType.EndOfFile)
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInObject, TokenType.EndOfFile, _tokens.Current.Type));

                        if (!headless && _tokens.Current.Type != TokenType.EndOfObject)
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInObject, TokenType.EndOfFile, _tokens.Current.Type));

                        switch (lastValue)
                        {
                            case null:
                                break;
                            case JsonPlusObjectMember _:
                                break;
                            default:
                                ((JsonPlusValue)jpObject.Parent).Add(lastValue.GetObject());
                                break;
                        }
                        lastValue = null;
                        parsing = false;
                        break;

                    default:
                        throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.ErrAtUnexpectedTokenInObject, _tokens.Current.Type));
                }
            }

            if (_tokens.Current.Type == TokenType.EndOfObject)
                _tokens.Next();

            return jpObject;
        }

        // parse path value
        private JsonPlusPath ParseKey()
        {
            while (_tokens.Current.LiteralType == LiteralTokenType.Whitespace)
            {
                _tokens.Next();
            }

            // sanity check
            if (_tokens.Current.Type != TokenType.LiteralValue)
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedKeyType, TokenType.LiteralValue, _tokens.Current.Type));

            if (_tokens.Current.IsNonSignificant())
                ConsumeWhitelines();
            if (_tokens.Current.Type != TokenType.LiteralValue)
                return null;

            TokenizeResult keyTokens = new TokenizeResult();
            while (_tokens.Current.Type == TokenType.LiteralValue)
            {
                keyTokens.Add(_tokens.Current);
                _tokens.Next();
            }

            keyTokens.Reverse();
            while (keyTokens.Count > 0 && keyTokens[0].LiteralType == LiteralTokenType.Whitespace)
            {
                keyTokens.RemoveAt(0);
            }
            keyTokens.Reverse();

            keyTokens.Add(new Token(string.Empty, TokenType.EndOfFile, null));

            return JsonPlusPath.FromTokens(keyTokens);
        }

        private JsonPlusObjectMember ParseObjectMember(JsonPlusObject owner)
        {
            // sanity check
            if(_tokens.Current.Type != TokenType.LiteralValue)
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInMember, TokenType.LiteralValue, _tokens.Current.Type));

            JsonPlusPath pathDelta = ParseKey();
            if (_tokens.Current.IsNonSignificant())
                ConsumeWhitelines();

            // sanity check
            if (_tokens.Current.Type != TokenType.Assignment &&
                _tokens.Current.Type != TokenType.StartOfObject &&
                _tokens.Current.Type != TokenType.SelfAssignment)
            {
                throw JsonPlusParserException.Create(_tokens.Current, Path,
                    string.Format(RS.UnexpectedTokenWith3AltInMember,
                        TokenType.Assignment, TokenType.StartOfObject, TokenType.SelfAssignment,
                        _tokens.Current.Type));
            }

            // sanity check
            if (pathDelta == null || pathDelta.Count == 0)
                throw JsonPlusParserException.Create(_tokens.Current, Path, RS.ObjectMemberPathUnspecified);

            List<JsonPlusObjectMember> childInPath = owner.TraversePath(pathDelta);

            Path.AddRange(pathDelta);
            JsonPlusObjectMember currentField = childInPath[childInPath.Count - 1];

            JsonPlusValue parsedValue = ParseValue(currentField);

            foreach (JsonPlusSubstitution removedSub in currentField.SetValue(parsedValue))
            {
                _substitutions.Remove(removedSub);
            }

            Path.RemoveRange(Path.Count - pathDelta.Count, pathDelta.Count);
            return childInPath[0];
        }

        /// <summary>
        /// Retrieves the next value token from the tokenizer and appends it
        /// to the supplied element <paramref name="owner"/>.
        /// </summary>
        /// <param name="owner">The element to append the next token.</param>
        /// <exception cref="Exception">End of file reached while trying to read a value</exception>
        private JsonPlusValue ParseValue(IJsonPlusNode owner)
        {
            JsonPlusValue value = new JsonPlusValue(owner);
            bool parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        value.Add(ParseInclude(value));
                        break;

                    case TokenType.LiteralValue:
                        // Consume leading whitespaces.
                        if (_tokens.Current.IsNonSignificant())
                            ConsumeWhitespace();
                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        while (_tokens.Current.Type == TokenType.LiteralValue)
                        {
                            value.Add(JsonPlusLiteralValue.Create(value, _tokens.Current));
                            _tokens.Next();
                        }
                        break;

                    case TokenType.StartOfObject:
                        value.Add(ParseObject(value));
                        break;

                    case TokenType.StartOfArray:
                        value.Add(ParseArray(value));
                        break;

                    case TokenType.OptionalSubstitution:
                    case TokenType.Substitution:
                        JsonPlusPath pointerPath = JsonPlusPath.Parse(_tokens.Current.Value);
                        JsonPlusSubstitution sub = new JsonPlusSubstitution(value, pointerPath, _tokens.Current,
                            _tokens.Current.Type == TokenType.Substitution);
                        _substitutions.Add(sub);
                        value.Add(sub);
                        _tokens.Next();
                        break;

                    case TokenType.EndOfObject:
                    case TokenType.EndOfArray:
                        parsing = false;
                        break;

                    // comments automatically stop value parsing.
                    case TokenType.Comment:
                        ConsumeWhitelines();
                        parsing = false;
                        break;

                    case TokenType.EndOfLine:
                        parsing = false;
                        break;

                    case TokenType.EndOfFile:
                    case TokenType.ArraySeparator:
                        parsing = false;
                        break;

                    case TokenType.SelfAssignment:
                        JsonPlusSubstitution subAssign = new JsonPlusSubstitution(value, new JsonPlusPath(Path), _tokens.Current, false);
                        _substitutions.Add(subAssign);
                        value.Add(subAssign);

                        value.Add(ParseSelfAssignArray(value));
                        parsing = false;
                        break;

                    case TokenType.Assignment:
                        ConsumeWhitelines();
                        break;

                    default:
                        throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.ErrAtUnexpectedToken, _tokens.Current.Type));
                }
            }

            // trim trailing whitespace if result is a literal
            if (value.Type == JsonPlusType.Literal)
            {
                if (value[value.Count - 1] is WhitespaceValue)
                    value.RemoveAt(value.Count - 1);
            }
            return value;
        }

        private IJsonPlusNode ParseSelfAssignArray(IJsonPlusNode owner)
        {
            // sanity check
            if (_tokens.Current.Type != TokenType.SelfAssignment)
                throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInArray, TokenType.SelfAssignment, _tokens.Current.Type));

            // consume += operator token
            ConsumeWhitelines();

            JsonPlusArray currentArray = new JsonPlusArray(owner);

            switch (_tokens.Current.Type)
            {
                case TokenType.Include:
                    // #todo
                    currentArray.Add(ParseInclude(currentArray));
                    break;

                case TokenType.StartOfArray:
                    return ParseArray(owner);

                case TokenType.StartOfObject:
                    // #todo
                    currentArray.Add(ParseObject(currentArray));
                    break;

                case TokenType.LiteralValue:
                    if (_tokens.Current.IsNonSignificant())
                        ConsumeWhitelines();
                    if (_tokens.Current.Type != TokenType.LiteralValue)
                        break;

                    currentArray.Add(ParseValue(currentArray));
                    return currentArray;

                case TokenType.OptionalSubstitution:
                case TokenType.Substitution:
                    JsonPlusPath pointerPath = JsonPlusPath.Parse(_tokens.Current.Value);
                    JsonPlusSubstitution sub = new JsonPlusSubstitution(owner, pointerPath, _tokens.Current,
                        _tokens.Current.Type == TokenType.Substitution);
                    _substitutions.Add(sub);
                    _tokens.Next();
                    return sub;

                default:
                    throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInArray, TokenType.EndOfArray, _tokens.Current.Type));
            }

            return currentArray;
        }

        /// <summary>
        /// Retrieves the next array token from the tokenizer.
        /// </summary>
        /// <returns>An array of elements retrieved from the token.</returns>
        private JsonPlusArray ParseArray(IJsonPlusNode owner)
        {
            JsonPlusArray currentArray = new JsonPlusArray(owner);

            // consume start of array token
            ConsumeWhitelines();

            IJsonPlusNode lastValue = null;
            bool parsing = true;
            while (parsing)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.Include:
                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path,
                                string.Format(RS.UnexpectedTokenWith2AltInArray,
                                    TokenType.ArraySeparator, TokenType.EndOfLine,
                                    _tokens.Current.Type));
                        }

                        lastValue = ParseInclude(currentArray);
                        break;

                    case TokenType.StartOfArray:
                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path,
                                string.Format(RS.UnexpectedTokenWith2AltInArray,
                                    TokenType.ArraySeparator, TokenType.EndOfLine,
                                    _tokens.Current.Type));
                        }

                        // Array inside of arrays are parsed as values because it can be value concatenated with another array.
                        lastValue = ParseValue(currentArray);
                        break;

                    case TokenType.StartOfObject:
                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path,
                                string.Format(RS.UnexpectedTokenWith2AltInArray,
                                    TokenType.ArraySeparator, TokenType.EndOfLine,
                                    _tokens.Current.Type));
                        }

                        lastValue = ParseObject(currentArray);
                        break;

                    case TokenType.LiteralValue:
                        if (_tokens.Current.IsNonSignificant())
                            ConsumeWhitespace();
                        if (_tokens.Current.Type != TokenType.LiteralValue)
                            break;

                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path,
                                string.Format(RS.UnexpectedTokenWith2AltInArray,
                                    TokenType.ArraySeparator, TokenType.EndOfLine,
                                    _tokens.Current.Type));
                        }

                        lastValue = ParseValue(currentArray);
                        break;

                    case TokenType.OptionalSubstitution:
                    case TokenType.Substitution:
                        if (lastValue != null)
                        {
                            throw JsonPlusParserException.Create(_tokens.Current, Path,
                                string.Format(RS.UnexpectedTokenWith2AltInArray,
                                    TokenType.ArraySeparator, TokenType.EndOfLine,
                                    _tokens.Current.Type));
                        }

                        lastValue = ParseValue(currentArray);

                        break;

                    case TokenType.Comment:
                    case TokenType.EndOfLine:
                        if (lastValue == null)
                        {
                            ConsumeWhitelines();
                            break;
                        }

                        currentArray.Add(lastValue);
                        lastValue = null;
                        ConsumeWhitelines();
                        break;

                    case TokenType.ArraySeparator:
                        if (lastValue == null)
                            throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.BadArrayType, _tokens.Current.Type));

                        currentArray.Add(lastValue);
                        lastValue = null;
                        ConsumeWhitelines();
                        break;

                    case TokenType.EndOfArray:
                        if (lastValue != null)
                        {
                            currentArray.Add(lastValue);
                            lastValue = null;
                        }
                        parsing = false;
                        break;

                    default:
                        throw JsonPlusParserException.Create(_tokens.Current, Path, string.Format(RS.UnexpectedTokenInArray, TokenType.EndOfArray, _tokens.Current.Type));
                }
            }

            // Consume end of array token
            _tokens.Next();
            return currentArray;
        }

        // be careful when using consume methods because it also consume the current token.
        private void ConsumeWhitespace()
        {
            while (_tokens.Next().Type != TokenType.EndOfFile)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.LiteralValue:
                        if (_tokens.Current.LiteralType == LiteralTokenType.Whitespace)
                            continue;
                        return;
                    case TokenType.Comment:
                        continue;
                    default:
                        return;
                }
            }
        }

        // be careful when using consume methods because it also consume the current token.
        private void ConsumeWhitelines()
        {
            while (_tokens.Next().Type != TokenType.EndOfFile)
            {
                switch (_tokens.Current.Type)
                {
                    case TokenType.LiteralValue:
                        if (_tokens.Current.LiteralType == LiteralTokenType.Whitespace)
                            continue;
                        return;
                    case TokenType.EndOfLine:
                    case TokenType.Comment:
                        continue;
                    default:
                        return;
                }
            }
        }
    }
}