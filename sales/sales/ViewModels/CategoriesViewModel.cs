﻿

namespace sales.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using sales.Helpers;
    using sales.Model;
    using sales.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using Xamarin.Forms;

    public class CategoriesViewModel : BaseViewModel
    {
        #region Attributes
        private string filter;

        private ApiService apiService;

        private bool isRefreshing;

        private ObservableCollection<CategoryItemViewModel> categories;
        #endregion

        #region Properties
        public string Filter
        {
            get { return this.filter; }
            set
            {
                this.filter = value;
                this.RefreshList();
            }
        }

        public List<Category> MyCategories { get; set; }

        public ObservableCollection<CategoryItemViewModel> Categories
        {
            get { return this.categories; }
            set { this.SetValue(ref this.categories, value); }
        }

        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set { this.SetValue(ref this.isRefreshing, value); }
        }
        #endregion

        #region Constructors
        public CategoriesViewModel()
        {
            this.apiService = new ApiService();
            this.LoadCategories();
        }
        #endregion

        #region Methods
        private async void LoadCategories()
        {
            this.IsRefreshing = true;

            var connection = await this.apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.IsRefreshing = false;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    connection.Message,
                    Languages.Accept);
                return;
            }

            var url = Application.Current.Resources["UrlApi"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlCategoriesController"].ToString();
            var response = await this.apiService.GetList<Category>(url, prefix, controller, Setting.AccessToken);
            if (!response.IsSuccess)
            {
                this.IsRefreshing = false;
                await Application.Current.MainPage.DisplayAlert(
                    Languages.Error,
                    response.Message,
                    Languages.Accept);
                return;
            }

            this.MyCategories = (List<Category>)response.Result;
            this.RefreshList();
            this.IsRefreshing = false;
        }

        private void RefreshList()
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                var myListCategoriesItemViewModel = this.MyCategories.Select(c => new CategoryItemViewModel
                {
                    categoryId = c.categoryId,
                    description = c.description,
                    imagePath = c.imagePath,
                });

                this.Categories = new ObservableCollection<CategoryItemViewModel>(
                    myListCategoriesItemViewModel.OrderBy(c => c.description));
            }
            else
            {
                var myListCategoriesItemViewModel = this.MyCategories.Select(c => new CategoryItemViewModel
                {
                    categoryId = c.categoryId,
                    description = c.description,
                    imagePath = c.imagePath,
                }).Where(c => c.description.ToLower().Contains(this.Filter.ToLower())).ToList();

                this.Categories = new ObservableCollection<CategoryItemViewModel>(
                    myListCategoriesItemViewModel.OrderBy(c => c.description));
            }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(RefreshList);
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new RelayCommand(LoadCategories);
            }
        }
        #endregion
    }
}
