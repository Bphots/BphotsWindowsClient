namespace HotsBpHelper.Utils.ComboBoxItemUtil
{
    public class ComboBoxItemInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Acronym { get; set; }

        public override string ToString()
        {
            return Name + Acronym;
        }
    }
}