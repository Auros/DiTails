using HMUI;
using IPA.Utilities;

namespace DiTails.Utilities
{
    internal static class Accessors
    {
        internal static readonly FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.Accessor DetailView = FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.GetAccessor("_standardLevelDetailView");
        internal static readonly FieldAccessor<StandardLevelDetailView, LevelBar>.Accessor LevelBar = FieldAccessor<StandardLevelDetailView, LevelBar>.GetAccessor("_levelBar");
        internal static readonly FieldAccessor<LevelBar, ImageView>.Accessor Artwork = FieldAccessor<LevelBar, ImageView>.GetAccessor("_songArtworkImageView");
    }
}