namespace Capriccioso
{
    [System.Serializable]
    /// <summary>
    /// A single folder from the CreateFolder build
    /// </summary>
    public class Folder
    {
        public string folderName = "";
        public Folder[] subFolders = new Folder[0];

        public Folder()
        {}
        public Folder(string f)
        {
            folderName = f;
        }

        public Folder(string folderName, Folder[] subFolders)
        {
            this.folderName = folderName;
            this.subFolders = new Folder[subFolders.Length];
            for (int i = 0; i < subFolders.Length; i++)
            {
                this.subFolders[i] = new Folder(subFolders[i].folderName);
            }
        }
    }
}