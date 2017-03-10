namespace Bjd.WebServer.Outside
{
    class EnvKeyValue 
    {
        public string Key { get; private set; }
        public string Val { get; private set; }
        public EnvKeyValue(string key, string val)
        {
            Key = key;
            Val = val;
        }
    }
}
