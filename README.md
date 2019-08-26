# IronBug
Project with several features for C# projects

## IronBug.Context

### TruncateTable

Truncate a table

```C#
var context = new YourContext();
context.Truncate<YourTable>();
```

### BulkInert

Insert data like a batch

```C#
var context = new YourContext();
context.BulkInsert(yourList);

// OR with options

var options = new DbContextOptions
{
    CommitCount = 1000,                   // Record amount saved per batch: default 10000 
    AutoDetectChangesEnabled = false,     
    ValidateOnSaveEnabled = false         
};
var context = new YourContext();
context.BulkInsert(yourList, options);
```

### DeleteAll

Delete all records in a table

```C#
var context = new YourContext();
context.DeleteAll<YourTable>();
```

### MakeAllStringsNonUnicode

Globally set varchar mapping over nvarchar

```C#
public class YourContext : DbContext
{    
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Add<MakeAllStringsNonUnicode>();
        base.OnModelCreating(modelBuilder);
    }
}
```

## IronBug.Pagination

### PagedList

Paged component

```C#
public ActinoResult Index(QueryFilter filter)
{
    var query = _context.Animals;
                
    if (!string.IsNullOrWhiteSpace(filter.Search))
        query = query.Where(q =>
            q.Name.Contains(filter.Search)
        );

    var pagedQuery = query.Paginate(filter);
    pagedQuery.AddSorterField(q => q.Id);
    pagedQuery.AddSorterField(q => q.Name);
    pagedQuery.AddSorterField(q => q.Age);

    return View(pagedQuery);
}
```
