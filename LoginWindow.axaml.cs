using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HelloAvalonia.Data;
using System.Security.Cryptography;
using System.Text;
using System;


namespace HelloAvalonia;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        Database.Initialize(); // Vérifie que la base est prête
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Merci de remplir tous les champs.");
            return;
        }

        var hashed = HashPassword(password);

        if (Database.ValidateLogin(username, hashed))
        {
            new MainWindow().Show();
            this.Close();
        }
        else
        {
            ShowMessage("Identifiants incorrects.");
        }
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes); // Nécessite using System;
    }

    private void OnGoToRegister(object? sender, RoutedEventArgs e)
    {
        new RegisterWindow().Show();
        this.Close();
    }

    private void ShowMessage(string msg)
    {
        var dialog = new Window
        {
            Width = 300,
            Height = 100,
            Content = new TextBlock
            {
                Text = msg,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
        dialog.ShowDialog(this);
    }
}