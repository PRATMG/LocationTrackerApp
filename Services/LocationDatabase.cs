using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using LocationTrackerApp.Models;

namespace LocationTrackerApp.Services
{
    public class LocationDatabase
    {
        private readonly SQLiteAsyncConnection _db;

        public LocationDatabase(string databasePath)
        {
            _db = new SQLiteAsyncConnection(databasePath);
        }

        public async Task InitializeAsync()
        {
            await _db.CreateTableAsync<LocationEntry>();
        }

        public Task<int> AddAsync(LocationEntry entry) => _db.InsertAsync(entry);

        public Task<List<LocationEntry>> GetAllAsync() =>
            _db.Table<LocationEntry>().OrderBy(e => e.TimestampUtc).ToListAsync();

        public Task<int> CountAsync() => _db.Table<LocationEntry>().CountAsync();

        public Task<int> ClearAsync() => _db.DeleteAllAsync<LocationEntry>();
    }
}
