using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreativeSpore.SuperTilemapEditor
{
    [System.Serializable]
    public class ParameterContainer
    {
        private const string k_warning_msg_wrongName = "Parameter with name {0} of type {1} and value {2} already exists";
        private const string k_warning_msg_paramNotFound = "Parameter with name {0} not found!";

        public Parameter this[string name] { get { return FindParam(name); } }

        [SerializeField]
        private List<Parameter> m_paramList = new List<Parameter>();

        public void AddNewParam(Parameter param, int idx = -1)
        {
            idx = idx >= 0 ? Mathf.Min(idx, m_paramList.Count) : m_paramList.Count;
            string origName = param.name;
            int i = 1;
            while (m_paramList.Exists(x => x.name == param.name))
            {
                param.name = origName + " (" + i + ")";
                ++i;
            }
            m_paramList.Insert(idx, param);
        }

        public void RemoveParam(string name)
        {
            m_paramList.RemoveAll(x => x.name == name);
        }

        public Parameter FindParam(string name)
        {
            return m_paramList.Find(x => x.name == name);
        }

        public void AddParam(string name, int value)
        {
            AddParam<int>(name, value);
        }

        public void AddParam(string name, float value)
        {
            AddParam<float>(name, value);
        }

        private void AddParam<T>(string name, T value)
        {
            Parameter param = FindParam(name);
            if (param != null)
            {
                string.Format(k_warning_msg_wrongName, param.name, param.GetParamType(), param.ToString());
            }
            else
            {
                if(value is bool)
                {
                    param = new Parameter(name, (bool)(System.Object)value);
                }
                else if (value is int)
                {
                    param = new Parameter(name, (int)(System.Object)value);
                }
                else if (value is float)
                {
                    param = new Parameter(name, (float)(System.Object)value);
                }
                else if (value is string)
                {
                    param = new Parameter(name, (string)(System.Object)value);
                }
                else if (value is UnityEngine.Object)
                {
                    param = new Parameter(name, (UnityEngine.Object)(System.Object)value);
                }
            }
            if (param != null)
            {
                m_paramList.Add(param);
            }
        }

        public bool GetBoolParam(string name, bool defaultValue = false)
        {
            Parameter param = FindParam(name);
            return param != null ? param.GetAsBool() : defaultValue;
        }

        public UnityEngine.Object GetObjectParam(string name, UnityEngine.Object defaultValue = null)
        {
            Parameter param = FindParam(name);
            return param != null ? param.GetAsObject() : defaultValue;
        }
    }

    public enum eParameterType
    {
        None,
        Bool,
        Int,
        Float,
        Object,
        String,
    }

    [Serializable]
    public class Parameter
    {
        private const string k_warning_msg_wrongType = "Parameter {0} of type {1} accessed as {2}";

        public string name;

        [SerializeField]
        private eParameterType _paramType = eParameterType.None;
        [SerializeField]
        private bool _boolValue = false;
        [SerializeField]
        private int _intValue = 0;
        [SerializeField]
        private float _floatValue = 0f;
        [SerializeField]
        private string _stringValue = string.Empty;
        [SerializeField]
        private UnityEngine.Object _objectValue = null;

        private Parameter(string name) { this.name = name; }
        public Parameter(string name, bool value) : this(name) { this._boolValue = value; _paramType = eParameterType.Bool; }
        public Parameter(string name, int value) : this(name) { this._intValue = value; _paramType = eParameterType.Int; }
        public Parameter(string name, float value) : this(name) { this._floatValue = value; _paramType = eParameterType.Float; }
        public Parameter(string name, string value) : this(name) { this._stringValue = value; _paramType = eParameterType.String; }
        public Parameter(string name, UnityEngine.Object value) : this(name) { this._objectValue = value; _paramType = eParameterType.Object; }

        public override string ToString()
        {
            switch (_paramType)
            {
                case eParameterType.Bool: return _boolValue.ToString();
                case eParameterType.Int: return _intValue.ToString();
                case eParameterType.Float: return _floatValue.ToString();
                case eParameterType.String: return _stringValue.ToString();
                case eParameterType.Object: return _objectValue.ToString();
                default: return "<Not defined>";
            }
        }

        public eParameterType GetParamType()
        {
            return _paramType;
        }

        public bool GetAsBool()
        {
            Debug.Assert(_paramType == eParameterType.Bool, string.Format(k_warning_msg_wrongType, name, _paramType, eParameterType.Bool));
            return _boolValue;
        }

        public UnityEngine.Object GetAsObject()
        {
            Debug.Assert(_paramType == eParameterType.Object, string.Format(k_warning_msg_wrongType, name, _paramType, eParameterType.Object));
            return _objectValue;
        }
    }
}
