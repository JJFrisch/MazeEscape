using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using MazeEscape.Models;
using MazeEscape.Persistence;

namespace MazeEscape
{ 
    public class LevelDatabase(string number)
    {
        public string DatabasePath { get; set; } = Path.Combine(FileSystem.AppDataDirectory, $"CampaignWorld{number}SQLite.db3");
        public SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;

        private SQLiteAsyncConnection database;

        private async Task Init()
        {
            if (database == null)
            {
                database = new SQLiteAsyncConnection(DatabasePath, Flags);
                await database.CreateTableAsync<CampaignLevelRecord>();
            }
        }

        public async Task<List<CampaignLevel>> GetLevelsAsync()
        {
            await Init();
            var records = await database.Table<CampaignLevelRecord>().ToListAsync();
            return records.Select(r => r.ToModel()).ToList();
        }

        public async Task AddNewLevelAsync(CampaignLevel c)
        {
            await Init();

            var record = CampaignLevelRecord.FromModel(c);
            record.LevelId = 0;
            var existing = await database.Table<CampaignLevelRecord>()
                .Where(i => i.LevelNumber == c.LevelNumber)
                .FirstOrDefaultAsync();

            if (existing is not null)
            {
                record.LevelId = existing.LevelId;
                await database.UpdateAsync(record);
                c.LevelID = record.LevelId;
                return;
            }

            await database.InsertAsync(record);
            c.LevelID = record.LevelId;
        }

        public async Task UpdateExistingLevelAsync(CampaignLevel c)
        {
            await Init();

            var record = CampaignLevelRecord.FromModel(c);
            if (record.LevelId != 0)
            {
                await database.UpdateAsync(record);
                return;
            }

            var existing = await database.Table<CampaignLevelRecord>()
                .Where(i => i.LevelNumber == c.LevelNumber)
                .FirstOrDefaultAsync();
            if (existing is null)
            {
                await database.InsertAsync(record);
                c.LevelID = record.LevelId;
                return;
            }

            record.LevelId = existing.LevelId;
            await database.UpdateAsync(record);
            c.LevelID = record.LevelId;
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
            if (c.LevelID != 0)
            {
                await database.DeleteAsync<CampaignLevelRecord>(c.LevelID);
                return;
            }

            var existing = await database.Table<CampaignLevelRecord>()
                .Where(i => i.LevelNumber == c.LevelNumber)
                .FirstOrDefaultAsync();
            if (existing is not null)
            {
                await database.DeleteAsync<CampaignLevelRecord>(existing.LevelId);
            }
        }

        public async Task DeleteAllLevelsAsync()
        {
            await Init();
            await database.DeleteAllAsync<CampaignLevelRecord>();
        }

        public async Task<CampaignLevel> GetItemAsync(string name)
        {
            await Init();
            var record = await database.Table<CampaignLevelRecord>().Where(i => i.LevelNumber == name).FirstOrDefaultAsync();
            return record?.ToModel();
        }

        //public async Task<List<CampaignLevel>> GetItemsNotDoneAsync()
        //{
        //    await Init();
        //    return await Database.Table<TodoItem>().Where(t => t.Done).ToListAsync();

        //    // SQL queries are also possible
        //    //return await Database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        //}
    }
}
