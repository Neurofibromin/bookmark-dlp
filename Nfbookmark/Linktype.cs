namespace Nfbookmark
{
    /// <summary>
    ///     Denotes what kind of youtube structure is linked.
    /// </summary>
    public enum Linktype 
    {
        /// <summary>
        /// regular video, default
        /// </summary>
        Video,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 4) == "user")
        /// </summary>
        Channel_user,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 7) == "channel")
        /// </summary>
        Channel_channel,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 1) == "@")
        /// </summary>
        Channel_at,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 2) == "c/")
        /// </summary>
        Channel_c,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 6) == "shorts")
        /// </summary>
        Short,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 8) == "playlist")
        /// </summary>
        Playlist,
        /// <summary>
        /// if (linkthatisbeingexamined.Substring(24, 7) == "results") //youtube search result was bookmarked
        /// </summary>
        Search 
    }
}