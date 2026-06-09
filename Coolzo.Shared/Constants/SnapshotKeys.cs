namespace Coolzo.Shared.Constants;

/// <summary>
/// STABLE KEY REGISTRY — the frozen public contract between the CMS publish pipeline and the public
/// Web portal. The portal binds to these keys; the published snapshot.json carries them.
///
/// RULES (enforced by review — see ProjectOverview SECTION 9 §7):
///  - A key here is permanent. Never rename or remove a key a portal page binds to without a
///    documented migration; add new keys instead.
///  - The published snapshot is a WORLD-READABLE static artifact. Only public-display data may be
///    keyed here — never PII, secrets, or internal-only fields.
///  - Theme values live as scalar rows in tblSystemSetting under the "theme." prefix.
///  - Content block / image slot keys follow the "page.slot" convention (lowercase, dot-separated).
/// </summary>
public static class SnapshotKeys
{
    /// <summary>Top-level JSON sections of the snapshot document.</summary>
    public static class Sections
    {
        public const string Theme = "theme";
        public const string Masters = "masters";
        public const string Content = "content";
        public const string Images = "images";
    }

    /// <summary>
    /// Theme tokens. Stored as tblSystemSetting rows keyed by these exact strings; composed into
    /// snapshot.theme and injected by the portal as CSS variables.
    /// </summary>
    public static class Theme
    {
        public const string Prefix = "theme.";

        public const string ColorPrimary = "theme.color.primary";
        public const string ColorAccent = "theme.color.accent";
        public const string ColorBackground = "theme.color.background";
        public const string ColorSurface = "theme.color.surface";
        public const string ColorBorder = "theme.color.border";
        public const string ColorTextPrimary = "theme.color.textPrimary";
        public const string ColorTextSecondary = "theme.color.textSecondary";
        public const string ColorSuccess = "theme.color.success";
        public const string ColorWarning = "theme.color.warning";
        public const string ColorError = "theme.color.error";
        public const string FontFamily = "theme.font.family";
        public const string FontWeights = "theme.font.weights";
        public const string LogoUrl = "theme.logoUrl";

        /// <summary>The complete, allowed set of theme token keys. Editing is restricted to these.</summary>
        public static readonly IReadOnlyCollection<string> AllKeys = new[]
        {
            ColorPrimary, ColorAccent, ColorBackground, ColorSurface, ColorBorder,
            ColorTextPrimary, ColorTextSecondary, ColorSuccess, ColorWarning, ColorError,
            FontFamily, FontWeights, LogoUrl
        };
    }

    /// <summary>Responsive breakpoints a screen image slot can target.</summary>
    public static class Breakpoints
    {
        public const string Desktop = "desktop";
        public const string Tablet = "tablet";
        public const string Mobile = "mobile";

        public static readonly IReadOnlyCollection<string> All = new[] { Desktop, Tablet, Mobile };
    }

    /// <summary>Master collection keys carried under snapshot.masters (populated from Phase 2 onward).</summary>
    public static class Masters
    {
        public const string Brands = "brands";
        public const string ServiceTypes = "serviceTypes";
        public const string ServiceSubTypes = "serviceSubTypes";
        public const string Pricing = "pricing";
        public const string AmcPlans = "amcPlans";
    }

    /// <summary>Content collection keys carried under snapshot.content.</summary>
    public static class Content
    {
        public const string Blocks = "blocks";
        public const string Banners = "banners";
        public const string Faqs = "faqs";
        public const string Testimonials = "testimonials";
    }

    /// <summary>Object-storage keys for the published artifact.</summary>
    public static class Storage
    {
        public const string LatestObjectKey = "cms/snapshot-latest.json";

        public static string VersionedObjectKey(int version) => $"cms/snapshot-{version}.json";
    }

    /// <summary>IMemoryCache key for the active snapshot manifest (fallback path).</summary>
    public const string ManifestCacheKey = "cms:snapshot:manifest";
}
