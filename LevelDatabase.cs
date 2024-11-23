using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using MazeEscape.Models;

namespace MazeEscape
{
    public class LevelDatabase
    {
        public string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "CampaignLevelSQLite.db3");
        public SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;

        private SQLiteAsyncConnection database;

        private async Task Init()
        {
            if (database == null)
            {
                database = new SQLiteAsyncConnection(DatabasePath, Flags);
                await database.CreateTableAsync<CampaignLevel>();
            }
        }

        public async Task<List<CampaignLevel>> GetLevelsAsync()
        {
            await Init();
            return await database.Table<CampaignLevel>().ToListAsync();
        }

        public async Task AddNewLevelAsync(CampaignLevel c)
        {
            if (c.LevelID != 0)
                return;
            await Init();
            await database.InsertAsync(c);
        }

        public async Task UpdateExistingLevelAsync(CampaignLevel c)
        {
            if (c.LevelID == 0)
                return;
            await Init();
            await database.UpdateAsync(c);
        }

        public async Task SaveLevelAsync(CampaignLevel c)
        {
            if (c.LevelID == 0) //New item
                await AddNewLevelAsync(c);
            else
                await UpdateExistingLevelAsync(c);
        }

        public async Task DeleteLevelAsync(CampaignLevel c)
        {
            await Init();
            await database.DeleteAsync(c);
        }

        public async Task DeleteAllLevelsAsync()
        {
            await Init();
            await database.DeleteAllAsync<CampaignLevel>();
        }


    }
}
