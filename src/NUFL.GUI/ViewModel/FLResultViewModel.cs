using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
using System.ComponentModel;

namespace NUFL.GUI.ViewModel
{
    public class FLResultViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public FLResultViewModel()
        {
            CovProgramChecked = true;
            SuspMethodChecked = true;
        }

        public List<ProgramEntityBase> CovResult
        {
            get
            {
                if (DataSource == null)
                {
                    return null;
                }
                Type gran = GetCovGranularity();
                if (gran == null)
                {
                    return null;
                }
                return DataSource.GetCovList(gran);
            }
        }

        public List<ProgramEntityBase> SuspResult
        {
            get
            {
                if (DataSource == null)
                {
                    return null;
                }
                Type gran = GetSuspGranularity();
                if (gran == null)
                {
                    return null;
                }
                return DataSource.GetSuspList(gran);
            }
        }

        #region Granularity


        Type GetSuspGranularity()
        {
            if(SuspClassChecked)
            {
                return typeof(Class);
            }
            if (SuspMethodChecked)
            {
                return typeof(Method);
            }
            if (SuspStatementChecked)
            {
                return typeof(InstrumentationPoint);
            }
            return null;
        }


        Type GetCovGranularity()
        {
            if (CovProgramChecked)
            {
                return typeof(Program);
            }
            if (CovModuleChecked)
            {
                return typeof(Module);
            }
            if (CovClassChecked)
            {
                return typeof(Class);
            }
            if (CovMethodChecked)
            {
                return typeof(Method);
            }
            return null;
        }


        bool _cov_program_checked;
        bool _cov_module_checked;
        bool _cov_class_checked;
        bool _cov_method_checked;
        void ClearCovCheck()
        {
            _cov_program_checked = false;
            _cov_module_checked = false;
            _cov_class_checked = false;
            _cov_method_checked = false;
        }
        public void OnCovGranularityChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("CovProgramChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("CovModuleChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("CovClassChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("CovMethodChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("CovResult"));
        }
        public bool CovProgramChecked
        {
            set
            {
                if(value)
                {
                    ClearCovCheck();
                    _cov_program_checked = value;
                } else
                {
                    _cov_program_checked = value;
                }
                OnCovGranularityChanged();
            }
            get
            {
                return _cov_program_checked;
            }
        }
        public bool CovModuleChecked
        {
            set
            {
                if (value)
                {
                    ClearCovCheck();
                    _cov_module_checked = value;
                }
                else
                {
                    _cov_module_checked = value;
                }
                OnCovGranularityChanged();
            }
            get
            {
                return _cov_module_checked;
            }
        }
        public bool CovClassChecked
        {
            set
            {
                if (value)
                {
                    ClearCovCheck();
                    _cov_class_checked = value;
                }
                else
                {
                    _cov_class_checked = value;
                }
                OnCovGranularityChanged();
            }
            get
            {
                return _cov_class_checked;
            }
        }
        public bool CovMethodChecked
        {
            set
            {
                if (value)
                {
                    ClearCovCheck();
                    _cov_method_checked = value;
                }
                else
                {
                    _cov_method_checked = value;
                }
                OnCovGranularityChanged();
            }
            get
            {
                return _cov_method_checked;
            }
        }


        
        bool _susp_class_checked;
        bool _susp_method_checked;
        bool _susp_statement_checked;
        void ClearSuspCheck()
        {
            _susp_class_checked = false;
            _susp_method_checked = false;
            _susp_statement_checked = false;
        }
        public void OnSuspGranularityChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("SuspClassChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("SuspMethodChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("SuspStatementChecked"));
            OnPropertyChanged(new PropertyChangedEventArgs("SuspResult"));
        }
        public bool SuspClassChecked
        {
            set
            {
                if (value)
                {
                    ClearSuspCheck();
                    _susp_class_checked = value;
                }
                else
                {
                    _susp_class_checked = value;
                }
                OnSuspGranularityChanged();
            }
            get
            {
                return _susp_class_checked;
            }
        }
        public bool SuspMethodChecked
        {
            set
            {
                if (value)
                {
                    ClearSuspCheck();
                    _susp_method_checked = value;
                }
                else
                {
                    _susp_method_checked = value;
                }
                OnSuspGranularityChanged();
            }
            get
            {
                return _susp_method_checked;
            }
        }
        public bool SuspStatementChecked
        {
            set
            {
                if (value)
                {
                    ClearSuspCheck();
                    _susp_statement_checked = value;
                }
                else
                {
                    _susp_statement_checked = value;
                }
                OnSuspGranularityChanged();
            }
            get
            {
                return _susp_statement_checked;
            }
        }
        #endregion



        RankList _data_source;
        public RankList DataSource 
        { 
            set
            {
                _data_source = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SuspResult"));
                OnPropertyChanged(new PropertyChangedEventArgs("CovResult"));
            }
            get
            {
                return _data_source;
            }
        }


    }
}
