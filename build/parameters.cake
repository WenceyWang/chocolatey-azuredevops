#load "./version.cake"

public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }
    public bool IsLocalBuild { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsMasterRepo { get; private set; }
    public bool IsMasterBranch { get; private set; }
    public bool IsDevelopBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool IsPublishBuild { get; private set; }
    public bool IsReleaseBuild { get; private set; }
    public bool SkipGitVersion { get; private set; }
    public BuildCredentials GitHub { get; private set; }
    public VisualStudioMarketplaceCredentials Marketplace { get; private set; }
    public WyamCredentials Wyam { get; private set; }
    public BuildVersion Version { get; private set; }

    public DirectoryPath WyamRootDirectoryPath { get; private set; }
    public DirectoryPath WyamPublishDirectoryPath { get; private set; }
    public FilePath WyamConfigurationFile { get; private set; }
    public string WyamRecipe { get; private set; }
    public string WyamTheme { get; private set; }
    public string WyamSourceFiles { get; private set; }
    public string WebHost { get; private set; }
    public string WebLinkRoot { get; private set; }
    public string WebBaseEditUrl { get; private set; }

    public IDictionary<string, object> WyamSettings
    {
        get
        {
            var settings =
                new Dictionary<string, object>
                {
                    { "Host",  WebHost },
                    { "LinkRoot",  WebLinkRoot },
                    { "BaseEditUrl", WebBaseEditUrl },
                    { "Title", "Chocolatey Azure DevOps" },
                    { "IncludeGlobalNamespace", false }
                };

            return settings;
        }
    }

    public bool ShouldPublishDocumentation
    {
        get
        {
            return !IsLocalBuild &&
                    !IsPullRequest &&
                    IsMasterRepo &&
                    (IsMasterBranch || IsDevelopBranch);
        }
    }

    public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMasterRepo
                && IsMasterBranch && IsTagged;
        }
    }

    public void SetBuildVersion(BuildVersion version)
    {
        Version  = version;
    }

    public bool CanUseWyam
    {
        get
        {
            return !string.IsNullOrEmpty(Wyam.AccessToken) &&
                !string.IsNullOrEmpty(Wyam.DeployRemote) &&
                !string.IsNullOrEmpty(Wyam.DeployBranch);
        }
    }

    public static BuildParameters GetParameters(
        ICakeContext context,
        BuildSystem buildSystem
        )
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        var target = context.Argument("target", "Default");

        return new BuildParameters {
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsRunningOnUnix = context.IsRunningOnUnix(),
            IsRunningOnWindows = context.IsRunningOnWindows(),
            IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor,
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsMasterRepo = StringComparer.OrdinalIgnoreCase.Equals("gep13/chocolatey-azuredevops", buildSystem.AppVeyor.Environment.Repository.Name),
            IsMasterBranch = StringComparer.OrdinalIgnoreCase.Equals("master", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsTagged = (
                buildSystem.AppVeyor.Environment.Repository.Tag.IsTag &&
                !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name)
            ),
            GitHub = new BuildCredentials (
                userName: context.EnvironmentVariable("CHOCOLATEYAZUREDEVOPS_GITHUB_USERNAME"),
                password: context.EnvironmentVariable("CHOCOLATEYAZUREDEVOPS_GITHUB_PASSWORD")
            ),
            Marketplace = new VisualStudioMarketplaceCredentials (
                token: context.EnvironmentVariable("CHOCOLATEYAZUREDEVOPS_VSMARKETPLACE_TOKEN")
            ),
            Wyam = new WyamCredentials (
                accessToken: context.EnvironmentVariable("WYAM_ACCESS_TOKEN"),
                deployRemote: context.EnvironmentVariable("WYAM_DEPLOY_REMOTE"),
                deployBranch: context.EnvironmentVariable("WYAM_DEPLOY_BRANCH")
            ),
            IsPublishBuild = new [] {
                "ReleaseNotes",
                "Create-Release-Notes"
            }.Any(
                releaseTarget => StringComparer.OrdinalIgnoreCase.Equals(releaseTarget, target)
            ),
            IsReleaseBuild = new string[] {
            }.Any(
                publishTarget => StringComparer.OrdinalIgnoreCase.Equals(publishTarget, target)
            ),
            SkipGitVersion = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CHOCOLATEYAZUREDEVOPS_SKIP_GITVERSION")),
            WyamRootDirectoryPath = context.MakeAbsolute(context.Environment.WorkingDirectory),
            WyamPublishDirectoryPath = context.MakeAbsolute(context.Directory("build-results/_PublishedDocumentation")),
            WyamConfigurationFile = context.MakeAbsolute((FilePath)"config.wyam"),
            WyamRecipe = "Blog",
            WyamTheme = "CleanBlog",
            WebHost = "gep13.github.io",
            WebLinkRoot = "/",
            WebBaseEditUrl = "https://github.com/gep13/chocolatey-azuredevops/tree/master/input/"
        };
    }
}

public class BuildCredentials
{
    public string UserName { get; private set; }
    public string Password { get; private set; }

    public BuildCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}

public class VisualStudioMarketplaceCredentials
{
    public string Token { get; private set; }

    public VisualStudioMarketplaceCredentials(string token)
    {
        Token = token;
    }
}

public class WyamCredentials
{
    public string AccessToken { get; private set; }
    public string DeployRemote { get; private set; }
    public string DeployBranch { get; private set; }

    public WyamCredentials(string accessToken, string deployRemote, string deployBranch)
    {
        AccessToken = accessToken;
        DeployRemote = deployRemote;
        DeployBranch = deployBranch;
    }
}