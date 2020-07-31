namespace Ranka.Utils
{
    // Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Helpers/AudioFile.cs
    public class MidoFile
    {
        private string m_FileName;
        private string m_Title;

        private string m_Thumbnail;
        private string m_Description;

        public MidoFile()
        {
            m_FileName = "";
            m_Title = "";
            m_Thumbnail = "";
            m_Description = "";
        }

        public override string ToString()
        {
            return m_Title;
        }

        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public string Thumbnail
        {
            get { return m_Thumbnail; }
            set { m_Thumbnail = value; }
        }

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }
    }
}