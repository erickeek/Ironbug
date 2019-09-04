# IronBug
Project with several features for C# projects

## IronBug.Context

### Truncate

Truncate a table

```C#
var context = new YourContext();
context.Truncate<YourTable>();
```

### BulkInert

Insert batch data

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

### RegisterAllTypeConfigurations

Dynamically register all type configurations

```C#
public class YourContext : DbContext
{    
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.RegisterAllTypeConfigurations(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

## IronBug.Helpers

### AttributeHelper
1. HasAttribute<>
2. Attributes<>
3. Attribute<>

### BooleanHelper
1. ToBoolean

### DecimalHelper
1. ToDecimal

### DictionaryHelper
1. AddRangeOverride
2. AddRangeNewOnly
3. AddRange
4. GetValueOrDefault

### EnumerableHelper
1. Except
2. AddRange
3. RemoveRange
4. Merge

### EnumHelper
1. GetValue
2. Attribute<>
3. DisplayName

### FileHelper
1. SaveToFile

### IntHelper
1. ToInt
2. Clamp

### StreamHelper
1. ToByteArray

### StringHelper
1. Truncate

### TypeHelper
1. GetClassProperties
2. IsClassValueType
3. IsAssignableTo<>

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
