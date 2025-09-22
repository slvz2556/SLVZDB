# SLVZDB, Simple Local File Database for C#

A lightweight, dependency-free library for storing and retrieving data locally in a file.  
This works like a very simplified ORM (similar to EF Core) but stores data in a plain text file — perfect for small projects, configs, or apps that don’t require a real database.

---

## Features

- Store and retrieve data without an external database
- Supports primitive data types (`int`, `string`, `bool`, `double`, `enum`, ...)
- Full CRUD operations (`Append`, `Get`, `Update`, `Remove`)
- Data stored in a human-readable plain text format
- Supports multiple models with separate database classes

---

## How to Use

### 1. Create Your Data Model
```csharp
enum Role
{
    Admin,
    User
}

class myModel
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Role Role { get; set; }
    public double Grade { get; set; }
    public bool IsActive { get; set; }
}

```
---
### 2. Create Your Database Context
```csharp
class myDB : DbContext<myModel>
{
    public override void Configuration()
    {
        SetConfig($"{Environment.CurrentDirectory}/slvz.db", "ID");
        // First argument: Database file full path
        // Second argument: Property name to be used as the unique key
    }
}


```
---


### 3. Example Usage
```csharp
var db = new myDB();

// Add 3 users
db.Append(new myModel
{
    ID = 1,
    Name = "Ashkan",
    Grade = 19.234,
    IsActive = true,
    Role = Role.Admin
});
db.Append(new myModel
{
    ID = 2,
    Name = "Arash",
    Grade = 21.992,
    IsActive = true,
    Role = Role.User
});
db.Append(new myModel
{
    ID = 3,
    Name = "Baran",
    Grade = 18.224452,
    IsActive = false,
    Role = Role.User
});

// Get all models
var models = db.Get();

// Modify one model
models.Find(x => x.ID == 3).IsActive = true;

// Update it in the database
db.Update(models.Find(x => x.ID == 3));

// Delete a model
// db.Remove(3);

// Get a single model by ID
var model = db.Get(3);

Console.WriteLine("Done");


```
---

## API Reference

### `Append(TModel model)`
Adds a single model to the database file.

- **Parameters:**  
  `model` — An instance of your model. Must match the configured model type.

---

### `Append(List<TModel> models)`
Adds multiple models to the database at once.

- **Parameters:**  
  `models` — A list of model instances. All must match the configured model type.

---

### `List<TModel> Get()`
Retrieves all models stored in the database.

- **Returns:**  
  A list of all models. Returns an empty list if no data file exists.

---

### `TModel Get(T id)`
Retrieves a single model by its key property.

- **Parameters:**  
  `key` — The value of the key property to search for.

- **Returns:**  
  The model matching the key, or a new default instance if not found.

---

### `Update(TModel model)`
Updates an existing model in the database.

- **Parameters:**  
  `model` — The updated model instance. The key property must match an existing entry.

---

### `Remove(T id)`
Deletes a model from the database by its key.

- **Parameters:**  
  `key` — The key value of the model to delete.
