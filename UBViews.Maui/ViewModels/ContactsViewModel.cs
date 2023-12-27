namespace UBViews.ViewModels;

using System;
using System.Resources;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;

using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Models.AppData;
using UBViews.Services;

public partial class ContactsViewModel : ObservableValidator
{
    /// <summary>
    /// CultureInfo
    /// </summary>
    public CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<ContactDto> Contacts { get; set; } = new();

    /// <summary>
    /// App Data Service
    /// </summary>
    IContactsService contactsService;

    /// <summary>
    /// IAppSettingsService
    /// </summary>
    IAppSettingsService settingsService;

    private readonly Regex _rgxEmail = new(@"^[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z_+])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9}$");

    private readonly string _class = "ContactsViewModel";

    private bool _firstNameValidString = false;
    private bool _lastNameValidString = false;
    private bool _displayNameValidString = false;
    private bool _emailValidString = false;
    private bool _validEmail = false;

    public ContactsViewModel(IContactsService contactsService, IAppSettingsService settingsService)
    {
        this.contactsService = contactsService;
        this.settingsService = settingsService;
    }

    [Required(ErrorMessage = "First Name is Required Field!")]
    [ObservableProperty]
    string firstName;

    [Required(ErrorMessage = "Last Name is Required Field!")]
    [ObservableProperty]
    string lastName;

    [Required(ErrorMessage = "Display Name is Required Field!")]
    [ObservableProperty]
    string displayName;

    [Required(ErrorMessage = "Email is Required Field!")]
    [ObservableProperty]
    string email;

    [ObservableProperty]
    bool autoSendEmail;

    [ObservableProperty]
    bool emailIsValid;

    [ObservableProperty]
    bool formIsValid;

    [ObservableProperty]
    bool contactSelected = false;

    [ObservableProperty]
    ContactDto selectedContact;

    [ObservableProperty]
    int contactsCount = 0;

    [ObservableProperty]
    string title;

    [ObservableProperty]
    bool displayNameValid = true;

    [ObservableProperty]
    bool contactFormCleared = false;

    [ObservableProperty]
    string errorMessage;

    [RelayCommand]
    async Task ContactsPageAppearing()
    {
        string _method = "ContactsPageAppearing";

        try
        {
            await GetContactList();
            FormIsValid = false;
            ContactSelected = false;
            SelectedContact = null;
            ContactsCount = Contacts.Count;
            Title = $"Contacts ({ContactsCount})";
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ContactsPageDisappearing()
    {
        string _method = "ContactsPageDisappearing";

        try
        {
            await contactsService.SaveContactsAsync();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task GetContactList()
    {
        string _method = "GetContactList";

        try
        {
            var _contacts = await contactsService.GetContactsAsync();
            Contacts.Clear();
            foreach (var contactDto in _contacts)
            {
                Contacts.Add(contactDto);
            }

            ContactsCount = Contacts.Count;
            Title = $"Contacts ({ContactsCount})";
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SaveContact(ContactDto selectedContact)
    {
        string _method = "ContactsViewModel.SaveContact";
        string message = string.Empty;

        try
        {
            var _contact = new ContactDto();
            await ValidateForm();

            if (FormIsValid)
            {
                _validEmail = !string.IsNullOrEmpty(Email) && _rgxEmail.Match(Email).Success;
                if (_validEmail)
                {
                    ContactFormCleared = false;
                    EmailIsValid = true;
                    _contact.FirstName = FirstName.Trim();
                    _contact.LastName = LastName.Trim();
                    _contact.DisplayName = DisplayName.Trim();
                    _contact.Email = Email.Trim();
                    _contact.AutoSendEmail = AutoSendEmail;

                    var _displayName = _contact.DisplayName;
                    var _displayNameIsValid = await ValidateDisplayName(_displayName);
                    if (!_displayNameIsValid)
                    {
                        message = $"Display name [{_displayName}] taken! Please enter another Display Name.";
                        await App.Current.MainPage.DisplayAlert($"Save Contact", message, "Ok");
                        return;
                    }

                    var id = await contactsService.SaveContactAsync(_contact);
                    Contacts.Add(_contact);

                    if (!ContactFormCleared)
                    {
                        await ClearContactForm();
                    }

                    ContactsCount = Contacts.Count;
                    Title = $"Contacts ({ContactsCount})";
                }
                else
                {
                    ErrorMessage = "Invalid email; please enter a valid email!";
                    await Shell.Current.DisplayAlert("Error!", ErrorMessage, "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error!", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task UpdateContact(ContactDto selectedContact)
    {
        string _method = "UpdateContact";
        string message = string.Empty;

        try
        {
            var _contact = new ContactDto();
            await ValidateForm();

            var _id = selectedContact.Id.ToString();
            var _oldContact = await contactsService.GetContactByIdAsync(_id);
            var _oldDisplayName = _oldContact.DisplayName;

            if (FormIsValid)
            {
                _validEmail = !string.IsNullOrEmpty(Email) && _rgxEmail.Match(Email).Success;
                if (_validEmail)
                {
                    ContactFormCleared = false;
                    EmailIsValid = true;
                    _contact.Id = selectedContact.Id;
                    _contact.FirstName = FirstName.Trim();
                    _contact.LastName = LastName.Trim();
                    _contact.DisplayName = DisplayName.Trim();
                    _contact.Email = Email.Trim();
                    _contact.AutoSendEmail = AutoSendEmail;

                    var _displayName = _contact.DisplayName;
                    var _isNewDisplayName = _oldDisplayName != _displayName;

                    if (_isNewDisplayName)
                    {
                        var _displayNameIsValid = await ValidateDisplayName(_displayName);
                        if (!_displayNameIsValid)
                        {
                            message = $"Display name [{_displayName}] taken! Please enter another Display Name.";
                            await App.Current.MainPage.DisplayAlert($"Save Contact", message, "Ok");
                            return;
                        }
                    }

                    var id = await contactsService.UpdateContactAsync(_contact);
                    await GetContactList();

                    if (!ContactFormCleared)
                    {
                        await ClearContactForm();
                    }
                }
                else
                {
                    ErrorMessage = "Invalid email; please enter a valid email!";
                    await Shell.Current.DisplayAlert("Error!", ErrorMessage, "OK");
                }
            }
            else
            {
                ErrorMessage = "There is an invalid field in the form.";
                await App.Current.MainPage.DisplayAlert($"InvalidForm!", ErrorMessage, "Ok");
            }

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task DeleteContact(ContactDto selectedContact)
    {
        string _method = "DeleteContact";
        string message = string.Empty;

        try
        {
            var id = await contactsService.DeleteContactAsync(selectedContact);
            Contacts.Remove(selectedContact);
            await ClearContactForm();

            ContactsCount = Contacts.Count;
            Title = $"Contacts ({ContactsCount})";

            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ClearContactForm()
    {
        string _method = "ClearContactFornm";

        try
        {
            var contactsCollection = contentPage.FindByName("ContactsCollection") as CollectionView;
            var firstNameEntry = contentPage.FindByName("FirstNameEntry") as Entry;
            var lastNameEntry = contentPage.FindByName("LastNameEntry") as Entry;
            var displayName = contentPage.FindByName("DisplayNameEntry") as Entry;
            var emailEntry = contentPage.FindByName("EmailEntry") as Entry;
            var autoSendCheckBox = contentPage.FindByName("AutoSendCheckBox") as CheckBox;

            firstNameEntry.Text = "";
            lastNameEntry.Text = "";
            displayName.Text = "";
            emailEntry.Text = "";
            autoSendCheckBox.IsChecked = false;

            ContactSelected = false;
            SelectedContact = null;
            FormIsValid = false;

            contactsCollection.SelectedItem = null;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                firstNameEntry.Focus();
                firstNameEntry.CursorPosition = 0;
            });
            ContactFormCleared = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SelectionChanged(object selectedItem)
    {
        string _method = "SelectionChanged";

        try
        {
            if (selectedItem == null)
            {
                return;
            }

            var contactsCollection = contentPage.FindByName("ContactsCollection") as CollectionView;
            var contact = selectedItem as ContactDto;
            var firstNameEntry = contentPage.FindByName("FirstNameEntry") as Entry;
            var lastNameEntry = contentPage.FindByName("LastNameEntry") as Entry;
            var displayName = contentPage.FindByName("DisplayNameEntry") as Entry;
            var emailEntry = contentPage.FindByName("EmailEntry") as Entry;
            var autoSendCheckBox = contentPage.FindByName("AutoSendCheckBox") as CheckBox;

            firstNameEntry.Text = contact.FirstName;
            lastNameEntry.Text = contact.LastName;
            displayName.Text = contact.DisplayName;
            emailEntry.Text = contact.Email;
            autoSendCheckBox.IsChecked = contact.AutoSendEmail;

            ContactSelected = true;
            SelectedContact = contact;
            FormIsValid = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task EmailTextChanged(string email)
    {
        string _method = "EmailTextChanged";

        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/displayNameEntry
        // TODO: bug, OnCompleted event doesn't fire on tab, only on return, not sure why?
        try
        {
            _validEmail = !string.IsNullOrEmpty(Email) && _rgxEmail.Match(Email).Success;
            if (_validEmail)
                EmailIsValid = true;
            else
                EmailIsValid = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task EmailEntryCompleted(string email)
    {
        string _method = "EmailEntryCompleted";

        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/displayNameEntry
        // TODO: bug, OnCompleted event doesn't fire on tab, only on return, not sure why?
        try
        {
            _validEmail = !string.IsNullOrEmpty(Email) && _rgxEmail.Match(Email).Success;
            if (_validEmail)
                EmailIsValid = true;
            else
                EmailIsValid = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AutoSendCheckedChanged(bool value)
    {
        string _method = "EmailEntryCompleted";

        try
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task<bool> ValidateDisplayName(string value)
    {
        string _method = "ValidDateOnValueChanged";

        try
        {
            var _length = value.Length;
            var displayNameEntry = contentPage.FindByName("DisplayNameEntry") as Entry;

            var bgc = displayNameEntry.BackgroundColor;
            var bgcStr = bgc.ToString();

            var _displayNameTaken = await contactsService.DisplayNameExistsAsync(value);
            if (_displayNameTaken)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (displayNameEntry != null)
                    {
                        displayNameEntry.BackgroundColorTo(Color.Parse("Azure"));
                        displayNameEntry.Focus();
                        displayNameEntry.CursorPosition = 0;
                        displayNameEntry.SelectionLength = _length;
                    }
                });
                DisplayNameValid = false;
            }
            else if (!_displayNameTaken && !DisplayNameValid)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (displayNameEntry != null)
                    {
                        displayNameEntry.BackgroundColorTo(Color.Parse("WhiteSmoke"));
                    }
                });

                if (!ContactFormCleared)
                {
                    await ClearContactForm();
                }
                DisplayNameValid = true;

            }
            return DisplayNameValid;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    [RelayCommand]
    async Task ValidateForm()
    {
        string _method = "ValidateForm";

        try
        {
            await Task.Run(() => { ValidateAllProperties(); });

            if (HasErrors)
                ErrorMessage = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
            else
                ErrorMessage = String.Empty;

            var errorDic = GetErrors().ToDictionary(k => k.MemberNames.First(), v => v.ErrorMessage);
            _firstNameValidString = !errorDic.TryGetValue(nameof(FirstName), out var errorFName);
            _lastNameValidString = !errorDic.TryGetValue(nameof(FirstName), out var errorLName);
            _displayNameValidString = !errorDic.TryGetValue(nameof(FirstName), out var errorDName);
            _emailValidString = !errorDic.TryGetValue(nameof(FirstName), out var errorEmail);
            if (_firstNameValidString && _lastNameValidString && _displayNameValidString && _emailValidString)
            {
                FormIsValid = true;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
}
