namespace NMDBase
{

    public struct NMDProject
    {
        public string Name { get; set; }
        public string NMDPath { get; set; }
        public int BoneCount { get; set; }
        public bool PendingSave { get; set; }
        public bool HasSaved { get; set; }
        public string LastSaved { get; set; }
        public string CreationMethod { get; set; }
        public string Template { get; set; }
        public string LastSelected { get; set; }
        public string RootParent { get; set; }
    }

}
