# IronBug
Project with several features for C# projects

## IronBug.Context

### Truncate

Truncate a table

```C#
var context = new YourContext();
context.Truncate<YourTable>();
```

### BulkInsert

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

### DateTimeHelper
1. ToDateTime
2. ToDateTimeOrNull
3. GetFirstDayOfMonth
4. GetLastDayOfMonth
5. ToShortDateString

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
1. SaveToFile (Save byte[] into a file)

### IntHelper
1. ToInt
2. Clamp

### StreamHelper
1. ToByteArray

### StringHelper
1. Truncate
2. ReplaceFirstOccurance (Replace first occurrence of a string)
3. ReplaceLastOccurance (Replace last occurrence of a string)
4. ToLowerFirstLetter (First letter in lowercase)
5. ToUpperFirstLetter (First letter in uppercase)
6. RemoveAccents (Remove diacritics (accents) from a string)
6. Nl2Br (A string extension method that newline 2 line break.)

### TypeHelper
1. GetClassProperties
2. IsClassValueType
3. IsAssignableTo<>

## IronBug.Pagination

### PagedList

Paged component

```C#
public ActionResult Index(QueryFilter filter)
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

## IronBug.Web

### IsDefinedInActionOrController

Checks if an attribute is defined on an action or controller

```C#
[Authorize]
public abstract class BaseController : Controller
{
	private bool IsRequestAuthorized(AuthorizationContext filterContext)
	{
		if (IsUserAuthenticated)
			return true;

		var descriptor = filterContext.ActionDescriptor;
		var authorize = descriptor.IsDefinedInActionOrController<AuthorizeAttribute>();
		var allowAnonymous = descriptor.IsDefinedInActionOrController<AllowAnonymousAttribute>();

		return !authorize || allowAnonymous;
	}
```

### ToJson

HtmlHelper to convert an object to JSON

```C#.cshtml
@Html.ToJson(Model)
```


### ContentVersioned

This extension method appends the assembly version to end of path

```C#.cshtml
<script src="@Url.ContentVersioned("~/js/service.js")"></script>
<script src="@Url.ContentVersioned("~/js/module.js")"></script>
```

Which will be rendered like this:
```xml
<script src="/js/service.js?v=1.0.0.0"></script>
<script src="/js/module.js?v=1.0.0.0"></script>
```


## IronBug.DomainValidation


```C#
public class UserMustBeAdmin : ISpecification
{
	private readonly User _user;

	public UserMustBeAdmin(User user) => _user = user;

	public bool IsSatisfiedBy() => _user.IsAdmin;

	public string ErrorMessage() => "You are not allowed to access this area";
}

public class ProposalMustBelongToTheUser : ISpecification
{
	private readonly User _user;
	private readonly Proposal _proposal;

	public ProposalMustBelongToTheUser(User user, Proposal proposal)
	{
		_user = user;
		_proposal = proposal;
	}

	public bool IsSatisfiedBy() => _proposal.IdUser = _user.Id;

	public string ErrorMessage() => "This proposal does not belong to you";
}


public sealed class UserCanAccessProposal : Validator
{
	public UserCanAccessProposal(User user, Proposal proposal)
	{
		Add(new UserMustBeAdmin(user));
		Add(new ProposalMustBelongToTheUser(user, proposal));
	}
}

// ...

public ActionResult Index(int id)
{
	// ...

	var result = new UserCanAccessProposal(user, proposal).Validate();
	if (!result.IsValid)
		throw new Exception(result.Message);

	// ...
}
```
