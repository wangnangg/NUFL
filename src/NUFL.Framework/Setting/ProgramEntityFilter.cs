using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  NUFL.Framework.Model;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;
namespace NUFL.Framework.Setting
{

    internal static class FilterHelper
    {
        internal static string WrapWithAnchors(this string data)
        {
            return String.Format("^({0})$", data);
        }

        internal static string ValidateAndEscape(this string match, string notAllowed = @"\[]")
        {
            if (match.IndexOfAny(notAllowed.ToCharArray()) >= 0) throw new InvalidOperationException(String.Format("The string is invalid for an filter name {0}", match));
            match = match.Replace(@"\", @"\\");
            match = match.Replace(@".", @"\.");
            match = match.Replace(@"*", @".*");
            return match;
        }
    }
    [Serializable]
    public enum FilterType
    {
        /// <summary>
        /// The filter is an inclusion type, i.e. if a assembly/class pair 
        /// matches the filter then it is included for instrumentation
        /// </summary>
        Inclusion,

        /// <summary>
        /// The filter is an exclusion type, i.e. if a assembly/class pair 
        /// matches the filter then it is excluded for instrumentation
        /// </summary>
        Exclusion
    }

     public class ProgramEntityFilter
     {
        
        List<KeyValuePair<string, string>> InclusionFilter { get; set; }
        List<KeyValuePair<string, string>> ExclusionFilter { get; set; }

        string _raw_filters;
        [XmlElement]
        public string RawFilters 
        {
            get
            {
                return _raw_filters;
            }
            set
            {
                var old_in = InclusionFilter;
                var old_ex = ExclusionFilter;
                var old_raw = _raw_filters;
                InclusionFilter = new List<KeyValuePair<string, string>>();
                ExclusionFilter = new List<KeyValuePair<string, string>>();
                _raw_filters = value;
                try
                {
                    string[] splits = _raw_filters.Split(',', ' ', '\n', '\r');
                    foreach (var filter in splits)
                    {
                        if (filter != "")
                        {
                            AddFilter(filter);
                        }
                    }
                } catch(Exception e)
                {
                    InclusionFilter = old_in;
                    ExclusionFilter = old_ex;
                    _raw_filters = old_raw;
                    throw new Exception("Invalid filters.");
                }
            }
        }

        public ProgramEntityFilter()
        {
            RawFilters = "+[*]*";
        }
        
        public bool UseAssembly(Module module)
        {

            string assemblyName = module.FullName;
            if (assemblyName == "")
            {
                return true;
            }
            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value == ".*"))
            {
                return false;
            }

            if (ExclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value != ".*"))
            {
                return true;
            }

            if (InclusionFilter.Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success))
            {
                return true;
            }

            return false;
        }

        public bool UseClass(Class @class)
        {
            string assemblyName = ((Module)@class.Parent).FullName;
            string className = @class.FullName;
            if (assemblyName == "")
            {
                return true;
            }
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(className))
            {
                return false;
            }

            if (ExclusionFilter
                .Any(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value == ".*"))
            {
                return false;
            }

            if (ExclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success && keyValuePair.Value != ".*")
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
            {
                return false;
            }

            if (InclusionFilter
                .Where(keyValuePair => Regex.Match(assemblyName, keyValuePair.Key.WrapWithAnchors()).Success)
                .Any(keyValuePair => Regex.Match(className, keyValuePair.Value.WrapWithAnchors()).Success))
            {
                return true;
            }

            return false;
        }

        public bool UseMethod(Method method)
        {
            return true;
        }


        void AddFilter(string assemblyClassName)
        {
            string assemblyName;
            string className;
            FilterType filterType;
            GetAssemblyClassName(assemblyClassName, out filterType, out assemblyName, out className);

            assemblyName = assemblyName.ValidateAndEscape();
            className = className.ValidateAndEscape();
            

            if (filterType == FilterType.Inclusion) 
                InclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));

            if (filterType == FilterType.Exclusion) 
                ExclusionFilter.Add(new KeyValuePair<string, string>(assemblyName, className));
        }

        private static void GetAssemblyClassName(string assemblyClassName, out FilterType filterType, out string assemblyName, out string className)
        {
            className = string.Empty;
            assemblyName = string.Empty;
            var regEx = new Regex(@"^(?<type>([+-]))(\[(?<assembly>(.+))\])(?<class>(.*))?$");

            var match = regEx.Match(assemblyClassName);
            if (match.Success)
            {
                filterType = match.Groups["type"].Value == "+" ? FilterType.Inclusion : FilterType.Exclusion;
                assemblyName = match.Groups["assembly"].Value;
                className = match.Groups["class"].Value;

                if (string.IsNullOrWhiteSpace(assemblyName))
                    throw new InvalidOperationException(string.Format("The supplied filter '{0}' does not meet the required format for a filter +-[assemblyname]classname", assemblyClassName));
            }
            else
            {
                throw new InvalidOperationException(string.Format("The supplied filter '{0}' does not meet the required format for a filter +-[assemblyname]classname", assemblyClassName));
            }
        }

    }
}
