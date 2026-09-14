using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using ToDo_Manager.Date;
using ToDo_Manager.Services;
using ToDo_Manager.Services.Interface;
using ToDo_Manager.View;
using ToDo_Manager.ViewModels;

namespace ToDo_Manager
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ToDoContext>();
                    services.AddSingleton<ITagService, TagService>();
                    services.AddSingleton<IMessageService, MessageService>();
                    services.AddTransient<IDialogService, DialogService>();
                    services.AddTransient<ITaskService, TaskService>();
                    services.AddTransient<EditTaskViewModel>();
                    services.AddDbContextFactory<ToDoContext>();

                    services.AddTransient<MainViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await AppHost.StartAsync();

               
                InitializeDatabase();

                var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup error: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            base.OnExit(e);
        }

        public static T GetService<T>() where T : class
            => AppHost.Services.GetRequiredService<T>();

        private void InitializeDatabase()
        {
            try
            {
                using var scope = AppHost.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ToDoContext>();

                
                context.ApplyMigrations();

                Console.WriteLine("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");

                
                try
                {
                    using var scope = AppHost.Services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ToDoContext>();

                    
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    Console.WriteLine("Database recreated successfully.");
                }
                catch (Exception recreateEx)
                {
                    Console.WriteLine($"Failed to recreate database: {recreateEx.Message}");
                    MessageBox.Show($"Database initialization failed: {recreateEx.Message}",
                        "Database Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}