using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HelloAvalonia.Data;
using System;
using System.Security.Cryptography;
using System.Text;

namespace HelloAvalonia;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        Database.Initialize(); // S'assure que la table Users existe
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var username = UsernameBox.Text?.Trim();
            var password = PasswordBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Merci de remplir tous les champs.");
                return;
            }

            if (Database.UserExists(username))
            {
                ShowMessage("Ce nom d'utilisateur est déjà pris.");
                return;
            }

            var hash = HashPassword(password);
            Database.AddUser(username, hash);

            ShowMessage("Compte créé avec succès !");

            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            ShowMessage($"Erreur : {ex.Message}");
        }
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private void ShowMessage(string message)
    {
        var dialog = new Window
        {
            Width = 300,
            Height = 100,
            Title = "Info",
            Content = new TextBlock
            {
                Text = message,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };

        dialog.ShowDialog(this);
    }
}
