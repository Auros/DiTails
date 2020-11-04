using HMUI;
using IPA.Utilities;

namespace DiTails.Utilities
{
    internal static class Accessors
    {
        internal static FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.Accessor DetailView = FieldAccessor<StandardLevelDetailViewController, StandardLevelDetailView>.GetAccessor("_standardLevelDetailView");
        internal static FieldAccessor<StandardLevelDetailView, LevelBar>.Accessor LevelBar = FieldAccessor<StandardLevelDetailView, LevelBar>.GetAccessor("_levelBar");
        internal static FieldAccessor<LevelBar, ImageView>.Accessor Artwork = FieldAccessor<LevelBar, ImageView>.GetAccessor("_songArtworkImageView");
    }
}