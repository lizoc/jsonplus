# #####################################################################
# Module manifest for module 'JsonPlus'
#
# Lizoc Inc.
# Last update: 2018-10-24
#
# This is a generated file. Modifications will be lost on the next 
# generate sequence.
#
# #####################################################################

@{
    # Script module or binary module file associated with this manifest.
    RootModule = 'Lizoc.PowerShell.JsonPlus.dll'

    # Version number of this module.
    ModuleVersion = '8.8.38.0'

    # ID used to uniquely identify this module
    GUID = 'd34ce124-413f-438f-8ad5-152fc451e8f0'

    # Author of this module
    Author = 'Powershell Team'

    # Company or vendor of this module
    CompanyName = 'Lizoc Inc.'

    # Copyright statement for this module
    Copyright = 'Copyright (c) 2018 Lizoc Inc. All rights reserved.'

    # Can run on these systems
    CompatiblePSEditions = @("Core")

    # Description of the functionality provided by this module
    Description = 'Parse Json+ to Powershell objects.'

    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '3.0'

    # Name of the Windows PowerShell host required by this module
    PowerShellHostName = ''

    # Minimum version of the Windows PowerShell host required by this module
    PowerShellHostVersion = '3.0'

    # Minimum version of Microsoft .NET Framework required by this module
    #DotNetFrameworkVersion = '4.5'

    # Minimum version of the common language runtime (CLR) required by this module
    #CLRVersion = '4.0'

    # Processor architecture (None, X86, Amd64) required by this module
    ProcessorArchitecture = 'None'

    # Modules that must be imported into the global environment prior to importing this module
    RequiredModules = @()

    # Assemblies that must be loaded prior to importing this module
    RequiredAssemblies = @(
        'Lizoc.JsonPlus.dll'
    )

    # Script files (.ps1) that are run in the caller's environment prior to importing this module.
    # ScriptsToProcess = @()

    # Type files (.ps1xml) to be loaded when importing this module
    TypesToProcess = @()

    # Format files (.ps1xml) to be loaded when importing this module
    FormatsToProcess = @()

    # First load importing this module. Depreciated (use 'RootModule').
    # ModuleToProcess = ''
    
    # Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
    # NestedModules = ''

    # Functions to export from this module
    # FunctionsToExport = @()

    # Cmdlets to export from this module
    CmdletsToExport = @(
        'ConvertFrom-JsonPlus'
    )

    # Variables to export from this module
    # VariablesToExport = @()

    # Aliases to export from this module
    # AliasesToExport = @()

    # List of all modules packaged with this module
    # ModuleList = @()

    # List of all files packaged with this module
    # FileList = @()

    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        # PSData is module packaging and gallery metadata embedded in PrivateData
        # It's for rebuilding NuGet-style packages
        # We had to do this because it's the only place we're allowed to extend the manifest
        # https://connect.microsoft.com/PowerShell/feedback/details/421837
        PSData = @{
            # The primary categorization of this module (from the TechNet Gallery tech tree).
            Category = 'Scripting Techniques'

            # Keyword tags to help users find this module via navigations and search.
            Tags = @('powershell', 'json+', 'jsonplus')

            # The web address of an icon which can be used in galleries to represent this module
            IconUri = 'https://raw.githubusercontent.com/lizoc/jsonplus/master/icon.png'

            # The web address of this module's project or support homepage.
            ProjectUri = 'https://www.github.com/lizoc/jsonplus'

            # The web address of this module's license. Points to a page that's embeddable and linkable.
            LicenseUri = 'https://raw.githubusercontent.com/lizoc/jsonplus/master/LICENSE'

            # Release notes for this particular version of the module
            ReleaseNotes = 'https://github.com/lizoc/jsonplus/blob/master/docs/known-issues.md'

            # If true, the LicenseUrl points to an end-user license (not just a source license) which requires the user agreement before use.
            RequireLicenseAcceptance = 'False'

            # Indicates this is a pre-release/testing version of the module.
            IsPrerelease = 'False'
        }

        # PSExtend is used by PowerExtend and its family products
        # https://www.github.com/lizoc/powerextend/blob/master/docs/about_psd_psextend.md
        PSExtend = @{
            # Indicates this is a pre-release/testing version of the module.
            DeployMode = 'Release'

            # Last update
            LastUpdate = '2018-10-24'

            # License family
            LicenseFamily = 'MIT'

            # Language customization
            # Language = 'System'
        }
    }

    # HelpInfo URI of this module
    HelpInfoURI = 'https://github.com/lizoc/jsonplus/blob/master/README.md'

    # Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
    # DefaultCommandPrefix = ''
}
