// MainWindow.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using HelloAvalonia.Data;
using System;
using System.Collections.ObjectModel;

namespace HelloAvalonia;

public partial class MainWindow : Window
{
    private ObservableCollection<string> rdvList = new();

    public MainWindow()
    {
        InitializeComponent();
        Database.Initialize();

        var rdvs = Database.ChargerTousLesRdvs();
        foreach (var rdv in rdvs)
            rdvList.Add(rdv);

        ListeRdv.ItemsSource = rdvList;
    }

    private void AjouterRdv(object? sender, RoutedEventArgs e)
    {
        var nom = NomTextBox.Text;
        var date = DatePicker.SelectedDate;
        var heure = HeureTextBox.Text;
        var description = DescriptionTextBox.Text;

        if (string.IsNullOrWhiteSpace(nom) || date == null || string.IsNullOrWhiteSpace(heure))
        {
            ShowMessage("Merci de remplir au minimum le nom, la date et l’heure.");
            return;
        }

        Database.AjouterRdv(nom, date.Value.DateTime, heure, description);
        var rdv = $"\ud83d\udcc5 {date:dd/MM/yyyy} | \ud83d\udd52 {heure} | \ud83d\udcac {nom} : {description}";
        rdvList.Add(rdv);

        NomTextBox.Text = "";
        HeureTextBox.Text = "";
        DescriptionTextBox.Text = "";
        DatePicker.SelectedDate = null;
    }

    private void SupprimerRdv(object? sender, RoutedEventArgs e)
    {
        if (ListeRdv.SelectedItem is string rdv)
        {
            Window? confirmation = null;

            var buttonYes = new Button
            {
                Content = "✅ Oui",
                Width = 100
            };

            var buttonCancel = new Button
            {
                Content = "❌ Annuler",
                Width = 100
            };

            confirmation = new Window
            {
                Width = 350,
                Height = 150,
                Title = "Confirmer la suppression",
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Spacing = 10,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Supprimer ce rendez-vous ?\n\n{rdv}",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            FontWeight = Avalonia.Media.FontWeight.Bold
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Spacing = 10,
                            Children =
                            {
                                buttonCancel,
                                buttonYes
                            }
                        }
                    }
                }
            };

            buttonCancel.Click += (_, _) => confirmation.Close();
            buttonYes.Click += (_, _) =>
            {
                // Extraire les champs de la chaîne de RDV
                var parts = rdv.Split('|');
                if (parts.Length >= 3)
                {
                    var date = parts[0].Replace("📅", "").Trim();
                    var heure = parts[1].Replace("🕒", "").Trim();
                    var nomEtDesc = parts[2].Replace("💬", "").Trim();
                    var nom = nomEtDesc.Split(':')[0].Trim();

                    Database.SupprimerRdv(nom, DateTime.ParseExact(date, "dd/MM/yyyy", null).ToString("yyyy-MM-dd"), heure);
                }

                rdvList.Remove(rdv);
                confirmation.Close();
            };

            confirmation.ShowDialog(this);
        }
        else
        {
            ShowMessage("Veuillez sélectionner un rendez-vous à supprimer.");
        }
    }

    private void ShowMessage(string message)
    {
        var dialog = new Window
        {
            Width = 300,
            Height = 100,
            Content = new TextBlock
            {
                Text = message,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        dialog.ShowDialog(this);
    }
    
    
    private void OuvrirCalendrier(object? sender, RoutedEventArgs e)
    {
        new CalendrierWindow().Show();
    }

}