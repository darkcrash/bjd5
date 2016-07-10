﻿using System;
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
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                System.Diagnostics.Trace.TraceError(ex.StackTrace);

            }
            _isMemberLoaded = true;
        }

        private struct FiledAndAtribute
        {
            public Attributes.ControlAttribute Control;
            public FieldInfo Field;
        }

        private List<FiledAndAtribute> GetControlFields(Type t)
        {
            var l = new List<FiledAndAtribute>();
            var fs = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var f in fs)
            {
                var attr = f.GetCustomAttribute<Attributes.ControlAttribute>(true);
                if (attr == null) continue;
                var r = new FiledAndAtribute();
                r.Control = attr;
                r.Field = f;
                l.Add(r);
            }
            return l;
        }

        private void MemberLoad(Type targetType, ListVal list, object instance)
        {
            var myType = targetType;
            if (myType.GenericTypeArguments.Length > 0)
            {
                var myTypeInGeneric = myType.GetGenericTypeDefinition();
                if (myTypeInGeneric == typeof(List<>))
                {
                    myType = myType.GenericTypeArguments[0];
                    instance = myType.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
            }

            foreach (var f in GetControlFields(myType))
            {
                if (f.Control.ControlType == CtrlType.Dat)
                {
                    var fValue = f.Field.GetValue(instance);
                    var childList = new ListVal();
                    var fType = f.Field.FieldType;

                    this.MemberLoad(fType, childList, fValue);

                    var dat = new Dat(childList);
                    var one = f.Control.Create(f.Field.Name, dat);
                    list.Add(one);
                }
                else
                {
                    var value = f.Field.GetValue(instance);
                    var one = f.Control.Create(f.Field.Name, value);
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

