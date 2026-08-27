using CafePos.Services.Interfaces;

namespace CafePos
{
    public partial class App : Application
    {
        private readonly IDbInitializer _dbInitializer;

        public App(IDbInitializer dbInitializer)
        {
            InitializeComponent();
            _dbInitializer = dbInitializer;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Asynchronously initialize database schema and product seed data on startup
            Task.Run(async () => await _dbInitializer.InitializeAsync());

            return new Window(new AppShell());
        }
    }
}