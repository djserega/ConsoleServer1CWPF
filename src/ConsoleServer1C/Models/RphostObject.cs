namespace ConsoleServer1C.Models
{
    public class RphostObject
    {
        public int PID { get; set; }
        public string SizeText { get; set; }
        public int Size { get; set; }

        public bool Fill(string data)
        {
            bool result = false;

            if (data?.StartsWith("rphost.exe") ?? false)
            {
                string fillData = data.Substring(10).TrimStart();

                string pid = string.Empty;
                foreach (char charData in fillData)
                {
                    if (charData == ' ')
                        break;

                    pid += charData;
                }

                PID = int.Parse(pid);
                SizeText = fillData
                    .Substring(pid.Length)
                    .TrimStart()
                    .Substring(1)
                    .TrimStart();
                Size = int.Parse(SizeText.Substring(0, SizeText.Length - 2).Replace(" ", "").Trim());

                result = true;
            }

            return result;
        }
    }
}
