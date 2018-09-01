using BentoEx.Common;
using BentoEx.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BentoEx.ViewModel
{
    public class MainViewModel : BindableBase
    {
        public ObservableCollection<Bento> Bentoes { get; set; }

        NetAccess Net;
        MyPassword Pass;

        private DateTime selectedDay = DateTime.Now;  // To be adjusted to Monday
        private readonly DateTime thisMonday;

        public MainViewModel()
        {
            Bentoes = new ObservableCollection<Bento>();

            Pass = new MyPassword();
            if (Pass.CompanyCode == null || Pass.UserId == null || Pass.Password == null)
            {
                var result = MessageBox.Show("You need to input login info first.", "Password not found", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    Process.Start("powershell.exe", @"-ExecutionPolicy Bypass -File .\SavePassword.ps1");
                }
                // Quit the program
                Application.Current.Shutdown();
                return;
            }

            Net = new NetAccess(Pass.CompanyCode, Pass.UserId, Pass.Password);

            // Adjust to Monday
            while (selectedDay.DayOfWeek != DayOfWeek.Monday)
            {
                if ((selectedDay.DayOfWeek == DayOfWeek.Friday && selectedDay.TimeOfDay > new TimeSpan(9, 45, 0))
                    || selectedDay.DayOfWeek == DayOfWeek.Saturday || selectedDay.DayOfWeek == DayOfWeek.Sunday)
                    selectedDay = selectedDay.AddDays(1);
                else
                    selectedDay = selectedDay.AddDays(-1);
            }
            thisMonday = selectedDay;
        }

        public void OnLoaded()
        {
            NeedBrowserInstall = !BrowserCheck.IsChromeInstalled();
            var task = Update(LoadMenu(selectedDay));
        }

        private async Task LoadMenu(DateTime date)
        {
            IsCheckAll = false;
            Bentoes.Clear();

            var bentos = await Net.GetBentoList(date);

            foreach (var item in bentos)
            {
                Bentoes.Add(item);
            }
        }

        private async Task Update(Task task)    // Access web with the updating indicator
        {
            try
            {
                IsUpdating = true;
                await task;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private DelegateCommand nextWeek;
        public DelegateCommand NextWeek
        {
            get
            {
                if (this.nextWeek == null)
                {
                    this.nextWeek = new DelegateCommand(NextWeekExecute, CanNextWeekExecute);
                }
                return this.nextWeek;
            }
        }
        private async void NextWeekExecute()
        {
            selectedDay = selectedDay.AddDays(7);
            await Update(LoadMenu(selectedDay));
        }
        private bool CanNextWeekExecute()
        {
            return selectedDay >= thisMonday.AddDays(14) ? false : true;
        }

        private DelegateCommand lastWeek;
        public DelegateCommand LastWeek
        {
            get
            {
                if (this.lastWeek == null)
                {
                    this.lastWeek = new DelegateCommand(LastWeekExecute, CanLastWeekExecute);
                }
                return this.lastWeek;
            }
        }
        private async void LastWeekExecute()
        {
            selectedDay = selectedDay.AddDays(-7);
            await Update(LoadMenu(selectedDay));
        }
        private bool CanLastWeekExecute()
        {
            return selectedDay <= thisMonday.AddDays(-14) ? false : true;
            //return true;
        }

        private DelegateCommand refreshMenu;
        public DelegateCommand RefreshMenu
        {
            get
            {
                if (this.refreshMenu == null)
                {
                    this.refreshMenu = new DelegateCommand(UpdateMenuExecute, CanUpdateMenuExecute);
                }
                return this.refreshMenu;
            }
        }
        private async void UpdateMenuExecute()
        {
            await Update(LoadMenu(selectedDay));
        }
        private bool CanUpdateMenuExecute()
        {
            return true;
        }

        private DelegateCommand submitOrder;
        public DelegateCommand SubmitOrder
        {
            get
            {
                if (this.submitOrder == null)
                {
                    this.submitOrder = new DelegateCommand(SubmitOrderExecute, CanSubmitOrderExecute);
                }
                return this.submitOrder;
            }
        }
        private async void SubmitOrderExecute()
        {
            var selenium = new BrowserAutomation(Pass.CompanyCode, Pass.UserId, Pass.Password);
            var dates= new List<DateTime>();

            foreach (var bt in Bentoes)
            {
                if (bt.ToBeOrdered)
                    dates.Add(bt.BentoDate);
            }

            await Update(selenium.OrderBentoes(dates));

            await Update(LoadMenu(selectedDay));
        }
        private bool CanSubmitOrderExecute()
        {
            if (NeedBrowserInstall)
                return false;

            foreach (var ben in Bentoes)
            {
                if (ben.ToBeOrdered)
                    return true;
            }
            return false;
        }

        private bool isCheckAll;
        public bool IsCheckAll
        {
            get { return isCheckAll; }
            set
            {
                isCheckAll = value;
                foreach (var ben in Bentoes)
                {
                    if (ben.CanOrder)
                        ben.ToBeOrdered = isCheckAll;
                }
                NotifyPropertyChanged();
            }
        }

        private bool isUpdating;
        public bool IsUpdating
        {
            get
            {
                return isUpdating;
            }
            private set
            {
                if (value == isUpdating)
                    return;

                isUpdating = value;
                NotifyPropertyChanged();
            }
        }

        private bool needBrowserInstall;
        public bool NeedBrowserInstall
        {
            get { return needBrowserInstall; }
            set
            {
                needBrowserInstall = value;
                NotifyPropertyChanged();
            }
        }
    }
}
