using System;
using System.Reflection;
using Bjd.Controls;
using Bjd.Net;
using Bjd.Utils;
using System.Collections.Generic;

namespace Bjd.Options
{
    abstract public class SmartOption : OneOption
    {

        private bool _isMemberLoaded = false;
        private readonly bool _isJp;


        public SmartOption(Kernel kernel, string path, string nameTag) : base(kernel, path, nameTag)
        {
            _isJp = kernel.IsJp;

        }

        private void MemberLoad()
        {
            if (_isMemberLoaded) return;
            try
            {
                var myType = this.GetType();
                MemberLoad(myType, ListVal, this);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);

            }
            _isMemberLoaded = true;
        }
        private void MemberLoad(Type targetType, ListVal list, object instance)
        {
            var myType = targetType;
            var fields = myType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var f in fields)
            {
                var attr = f.GetCustomAttribute<Attributes.ControlAttribute>(true);
                if (attr == null) continue;
                if (attr.ControlType == CtrlType.Dat)
                {
                    var fType = f.FieldType;
                    if (fType.DeclaringType == typeof(List<>))
                    {
                        fType = fType.GenericTypeArguments[0];
                    }
                    var value = f.GetValue(instance);
                    var childList = new ListVal();
                    this.MemberLoad(fType, childList, value);
                    var dat = new Dat(childList);
                    var one = attr.Create(f.Name, dat);
                    list.Add(one);
                }
                else
                {
                    var value = f.GetValue(instance);
                    var one = attr.Create(f.Name, value);
                    list.Add(one);
                }

            }

        }



        //レジストリへ保存
        public override void Save(IniDb iniDb)
        {
            iniDb.Save(NameTag, ListVal);//レジストリへ保存
        }


        //レジストリからの読み込み
        public override void Read(IniDb iniDb)
        {
            MemberLoad();
            iniDb.Read(NameTag, ListVal);
        }

        public override void Dispose()
        {

        }
    }
}

