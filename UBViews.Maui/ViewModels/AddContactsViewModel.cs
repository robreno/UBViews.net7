namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Models.AppData;
using UBViews.Services;

public partial class AddContactsViewModel : ObservableValidator
{
    /// <summary>
    /// CultureInfo
    /// </summary>
    public CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage and ConttentView
    /// </summary>
    public ContentPage contentPage;
    public ContentView contentView;

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<ContactDto> Contacts { get; private set; } = new();

    /// <summary>
    /// App Data Service
    /// </summary>
    IContactsService contactsService;

    /// <summary>
    /// IAppSettingsService
    /// </summary>
    IAppSettingsService settingsService;

    private readonly Regex _rgxEmail = new(@"^[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z_+])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9}$");

    private bool _firstNameValidString = false;
    private bool _lastNameValidString = false;
    private bool _displayNameValidString = false;
    private bool _emailValidString = false;
    private bool _validEmail = false;

    public AddContactsViewModel(IContactsService contactsService, IAppSettingsService settingsService)
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
    string errorMessage;

    [RelayCommand]
    async Task AddContactsPageAppearing()
    {
        try
        {
            var contacts = await contactsService.GetContactsAsync();
            foreach (var contactDto in contacts)
            {
                Contacts.Add(contactDto);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task AddContactsPageLoaded()
    {
        try
        {
            FormIsValid = false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task AddContactsPageDisappearing()
    {
        try
        {
            await contactsService.SaveContactsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task AddContactsPageUnloaded()
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task EmailTextChanged(string email)
    {
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/entry
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
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task EmailEntryCompleted(string email) 
    {
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/entry
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
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task AutoSendCheckedChanged(bool value)
    {
        try
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task SaveContact()
    {
        // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/entry
        // TODO: bug, OnCompleted event doesn't fire on tab, only on return, not sure why?
        // https://stackoverflow.com/questions/74989278/how-do-i-get-net-maui-validation-working
        // https://learn.microsoft.com/en-us/dotnet/architecture/maui/validation

        // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/behaviors/text-validation-behavior

        try
        {
            var _contact = new ContactDto();
            await Validate();
            
            if (FormIsValid)
            {
                _validEmail = !string.IsNullOrEmpty(Email) && _rgxEmail.Match(Email).Success;
                if (_validEmail)
                {
                    EmailIsValid = true;
                    _contact.FirstName = FirstName.Trim();
                    _contact.LastName = LastName.Trim();
                    _contact.DisplayName = DisplayName.Trim();
                    _contact.Email = Email.Trim();
                    _contact.AutoSendEmail = AutoSendEmail;
                    var id = await contactsService.SaveContactAsync(_contact);
                    _contact.Id = id;
                    Contacts.Add(_contact);
                    if (EmailIsValid && AutoSendEmail)
                    {
                        var recipients = await settingsService.Get("auto_send_list", "");
                        recipients = recipients + _contact.Email + ";";
                        await settingsService.Set("auto_send_list", recipients);
                    }
                    await ClearForm();
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
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    async Task ClearForm()
    {
        try
        {
            var fname = contentView.FindByName("FirstNameEntry") as Entry;
            var lname = contentView.FindByName("LastNameEntry") as Entry;
            var dname = contentView.FindByName("DisplayNameEntry") as Entry;
            var email = contentView.FindByName("EmailEntry") as Entry;
            var autosend = contentView.FindByName("AutoSendCheckBox") as CheckBox;
            var fullname = fname.Text + " " + lname.Text;
            fname.Text = "";
            lname.Text = "";
            dname.Text = "";
            email.Text = "";
            autosend.IsChecked = false;
            var msg = $"For {fullname}";
            await Shell.Current.DisplayAlert("Contact Saved!", msg, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task Validate()
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
}
