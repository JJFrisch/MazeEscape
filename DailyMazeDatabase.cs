using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using MazeEscape.Models;

namespace MazeEscape
{
    public class DailyMazeDatabase
    {
        public string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "DailyMazeLevelsSQLite.db3");
        public SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;

        private SQLiteAsyncConnection database;

        private async Task Init()
        {
            if (database == null)
            {
                database = new SQLiteAsyncConnection(DatabasePath, Flags);
                await database.CreateTableAsync<DailyMazeLevel>();
            }
        }

        public async Task<List<DailyMazeLevel>> GetLevelsAsync() 
        {
            await Init();
            return await database.Table<DailyMazeLevel>().ToListAsync();
        }

        public async Task AddNewLevelAsync(DailyMazeLevel c)
        {
            if (c.LevelID != 0)
                return;
            await Init();
            await database.InsertAsync(c);
        }
          
        public async Task UpdateExistingLevelAsync(DailyMazeLevel c)
        {
            if (c.LevelID == 0)
                return;
            await Init();
            await database.UpdateAsync(c);
        }

        public async Task SaveLevelAsync(DailyMazeLevel c)
        {
            if (c.LevelID == 0) //New item
                await AddNewLevelAsync(c);
            else
                await UpdateExistingLevelAsync(c);
        }

        public async Task DeleteLevelAsync(DailyMazeLevel c)
        {
            await Init();
            await database.DeleteAsync(c);
        }

        public async Task DeleteAllLevelsAsync()
        {
            await Init();
            await database.DeleteAllAsync<DailyMazeLevel>();
        }

        public async Task<DailyMazeLevel> GetItemAsync(string shortDate)
        {
            await Init();

            //List<DailyMazeLevel> x = await GetLevelsAsync();
            //if (x.Count == 0)
            //{
            //    return null;
            //}
            return await database.Table<DailyMazeLevel>().Where(i => i.ShortDate == shortDate).FirstAsync();
            //return await database.Table<DailyMazeLevel>().Where(i => i.TimeNeeded == 0).FirstOrDefaultAsync();
        }
         

        public async Task<List<DailyMazeLevel>> GetItemsOfMonthYearAsync(string monthYear) // Date.ToString("MM-yyyy")
        {
            await Init();
            return await database.Table<DailyMazeLevel>().Where(t => t.TimeNeeded == 0).ToListAsync();
            //return await database.QueryAsync<DailyMazeLevel>("SELECT * FROM [DailyMazeLevel] WHERE [Month_Year] = " + monthYear); // Date.ToString("MM-yyyy")

        }

        //public async Task<List<DailyMazeLevel>> GetItemsNotDoneAsync()
        //{
        //    await Init();
        //    return await Database.Table<TodoItem>().Where(t => t.Done).ToListAsync();

        //    // SQL queries are also possible
        //    //return await Database.QueryAsync<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
        //}
    }
}
