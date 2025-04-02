// CalendrierWindow.axaml.cs

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using HelloAvalonia.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HelloAvalonia;

public partial class CalendrierWindow : Window
{
    private DateTime _moisActuel;
    private TextBlock _moisTitre;

    public CalendrierWindow()
    {
        InitializeComponent();
        _moisActuel = DateTime.Today;
        GenererCalendrier(_moisActuel);
    }

    private void GenererCalendrier(DateTime mois)
    {
        var grille = this.FindControl<Grid>("CalendrierGrille");
        grille.RowDefinitions.Clear();
        grille.ColumnDefinitions.Clear();
        grille.Children.Clear();

        _moisTitre = new TextBlock
        {
            Text = mois.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
            FontSize = 20,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(_moisTitre, 0);
        Grid.SetColumnSpan(_moisTitre, 7);
        grille.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grille.Children.Add(_moisTitre);

        var boutons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                new Button
                {
                    Content = "⬅️ Mois précédent",
                    Width = 150,
                    Height = 30,
                    Margin = new Thickness(5)
                },
                new Button
                {
                    Content = "➡️ Mois suivant",
                    Width = 150,
                    Height = 30,
                    Margin = new Thickness(5)
                }
            }
        };

        var btnPrev = (Button)boutons.Children[0];
        var btnNext = (Button)boutons.Children[1];

        btnPrev.Click += (_, _) =>
        {
            _moisActuel = _moisActuel.AddMonths(-1);
            GenererCalendrier(_moisActuel);
        };

        btnNext.Click += (_, _) =>
        {
            _moisActuel = _moisActuel.AddMonths(1);
            GenererCalendrier(_moisActuel);
        };

        Grid.SetRow(boutons, 1);
        Grid.SetColumnSpan(boutons, 7);
        grille.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grille.Children.Add(boutons);

        // En-têtes des jours de la semaine
        var jours = new[] { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };
        grille.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (int i = 0; i < 7; i++)
        {
            grille.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var titre = new TextBlock
            {
                Text = jours[i],
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(titre, 2);
            Grid.SetColumn(titre, i);
            grille.Children.Add(titre);
        }

        var premierJour = new DateTime(mois.Year, mois.Month, 1);
        var decalage = ((int)premierJour.DayOfWeek + 6) % 7; // Lundi = 0
        var nbJours = DateTime.DaysInMonth(mois.Year, mois.Month);

        var rdvs = Database.ChargerTousLesRdvs();
        var rdvsParDate = new Dictionary<string, List<string>>();

        foreach (var rdv in rdvs)
        {
            var date = rdv.Split('|')[0].Replace("📅", "").Trim();
            if (!rdvsParDate.ContainsKey(date)) rdvsParDate[date] = new();
            rdvsParDate[date].Add(rdv);
        }

        int jour = 1, ligne = 3;
        while (jour <= nbJours)
        {
            grille.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (int col = 0; col < 7; col++)
            {
                if (ligne == 3 && col < decalage) continue;
                if (jour > nbJours) break;

                var stack = new StackPanel { Margin = new Thickness(4), Spacing = 2 };
                var jourLabel = new TextBlock
                {
                    Text = jour.ToString(),
                    FontWeight = Avalonia.Media.FontWeight.Bold
                };
                stack.Children.Add(jourLabel);

                var key = new DateTime(mois.Year, mois.Month, jour).ToString("dd/MM/yyyy");
                if (rdvsParDate.ContainsKey(key))
                {
                    foreach (var r in rdvsParDate[key])
                    {
                        stack.Children.Add(new TextBlock
                        {
                            Text = "- " + r.Split('|')[2].Trim(),
                            FontSize = 11,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        });
                    }
                }

                var border = new Border
                {
                    BorderBrush = Avalonia.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(4),
                    Child = stack
                };

                border.PointerPressed += (_, _) =>
                {
                    if (rdvsParDate.ContainsKey(key))
                    {
                        var contenu = string.Join("\n", rdvsParDate[key]);
                        var dialog = new Window
                        {
                            Width = 400,
                            Height = 300,
                            Title = $"RDV du {key}",
                            Content = new ScrollViewer
                            {
                                Content = new TextBlock
                                {
                                    Text = contenu,
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                    Margin = new Thickness(10)
                                }
                            }
                        };
                        dialog.ShowDialog(this);
                    }
                };

                Grid.SetRow(border, ligne);
                Grid.SetColumn(border, col);
                grille.Children.Add(border);
                jour++;
            }
            ligne++;
        }
    }
}