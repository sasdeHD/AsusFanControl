using AsusFanControl.Infrastructure;
using AsusFanControl.Model;


namespace AsusFanControl.Service
{
    public class SettingService
    {
        private AppDbContext db;

        public SettingService(AppDbContext db)
        {
            this.db = db;
        }

        public AppSetting GetSetting()
        {
            return db.Setting.Single();
        }

        public void SaveSetting(AppSetting setting)
        {
            var appSetting = db.Setting.Single();
            db.Entry(appSetting).CurrentValues.SetValues(setting);
            db.SaveChanges();
        }

    }
}
