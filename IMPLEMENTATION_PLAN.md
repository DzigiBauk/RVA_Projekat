# Implementation Plan — Aquarium Aquatic Species Monitoring System

## Summary

Build two connected .NET 10 WPF applications using MVVM:

- **Component 1:** Manage aquatic species and water-quality readings, persistence, logging, undo/redo, state simulation, and real-time charts.
- **Component 2:** Retrieve filtered readings through CoreWCF, map them into the required dictionary, calculate statistics, display results, and export CSV.
- **Shared:** Hold data contracts, enums, DTOs, and the CoreWCF service contract.

Code and identifiers will use English. Both user interfaces will also use English.

## Phase 1 — Solution Architecture and Dependencies

- Organize both applications into `Models`, `Views`, `ViewModels`, `Services`, `Interfaces`, `Commands`, `Helpers`, and `Converters`.
- Add project references from both components to `Shared`.
- Configure:
  - CoreWCF HTTP hosting in Component 1.
  - System.ServiceModel client packages in Component 2.
  - `LiveChartsCore.SkiaSharpView.WPF` in Component 1.
  - MVVM support through reusable observable and command base classes.
- Keep business logic, file access, communication, and UI logic behind interfaces to support SOLID and unit testing.
- Use constructor injection through a small application composition root.

## Phase 2 — Shared Domain and Communication Contracts

Define:

- `AquaticSpecies`
  - `Guid Id`
  - `string Name`
  - `string ScientificName`
  - `string Habitat`
  - `string WaterType`
  - `int AverageLifespan`
- `WaterQualityReading`
  - `Guid Id` — internal technical identifier
  - `Guid SpeciesId`
  - `DateTime MeasurementTime`
  - `double PHLevel`
  - `double Temperature`
  - `double OxygenLevel`
  - `WaterQualityState State`
- `WaterQualityState`
  - `Optimal`
  - `Acceptable`
  - `Suboptimal`
  - `Critical`

Add CoreWCF-compatible DTOs and service contract:

- `GetSpeciesAsync()` returns species available for selection.
- `GetReadingsAsync(ReadingsRequest request)` returns readings for one species, year, and month.
- `ReadingsRequest` contains `SpeciesId`, `Year`, and `Month`.
- `SpeciesReadingsResponse` contains species identity/name and matching readings.
- Use year together with month to prevent ambiguity between records from different years.
- Communication DTOs must not expose WPF-specific or persistence-specific types.

## Phase 3 — Component 1 Data and Persistence

- Implement repository interfaces for species and readings.
- Store all application data in a readable JSON file.
- Load data during application startup.
- If the file is missing, empty, or contains no usable records, create at least:
  - Three aquatic species.
  - Three water-quality readings.
- If JSON is invalid:
  - Preserve or rename the damaged file.
  - Load seed data.
  - Record the failure in the application log.
  - Show a non-fatal warning.
- Save automatically after successful CRUD, undo, and redo operations.
- Also provide an explicit “Save Data” action.
- Write files atomically through a temporary file to reduce corruption risk.

## Phase 4 — Component 1 CRUD, Search, and Validation

Create separate species and reading management screens using DataGrids.

### Species operations

- Display every required property.
- Add, edit, and delete species.
- Search across ID, name, scientific name, habitat, water type, and lifespan.
- When deleting a species with readings:
  - Request confirmation.
  - Cascade-delete its readings.
  - Preserve the entire operation as one undoable action.

### Reading operations

- Display every required property, including resolved species name.
- Add, edit, and delete readings.
- Search across reading ID, species, date/time, numeric values, and state.
- Select species from an existing-species dropdown rather than entering a raw GUID.

### Validation

- Require all textual species fields.
- Restrict `WaterType` to Freshwater, Saltwater, or Brackish while retaining the required string property.
- Require a positive, reasonable lifespan.
- Require an existing `SpeciesId`.
- Validate pH within `0–14`.
- Validate temperature and oxygen as finite values within documented aquarium-safe input limits.
- Reject duplicate reading IDs.
- Display validation next to fields and disable saving while errors exist.
- Keep validation rules in reusable validators rather than code-behind.

## Phase 5 — Undo/Redo and Activity Logging

- Apply the Command pattern to add, edit, delete, and cascade-delete operations.
- Maintain independent undo and redo stacks in the Component 1 session.
- Clear the redo stack after a new user mutation.
- Restore exact previous values and collection positions where practical.
- Expose `CanUndo` and `CanRedo` to buttons and keyboard shortcuts:
  - `Ctrl+Z`
  - `Ctrl+Y`
- Do not add automatic state transitions to the user undo stack.

Log the following:

- Application startup and shutdown.
- Data loading and saving.
- Add, edit, and delete operations.
- Undo and redo.
- State transitions.
- WCF requests and communication errors.
- Persistence and validation failures.

Each text-log entry contains timestamp, severity, and a descriptive message. Logging failures must not crash the application.

## Phase 6 — State Simulation and Real-Time Chart

- Create a state simulation service independent of the ViewModel.
- Register loaded and newly created readings with the simulator.
- Advance each reading on a configurable five-second interval:
  - `Optimal → Acceptable → Suboptimal → Critical → Optimal`
- Marshal updates onto the WPF UI thread.
- Stop simulation for deleted readings and dispose timers on shutdown.
- Persist changed states without writing on every timer tick; use debounced periodic persistence and a final shutdown save.
- Update visible DataGrid state immediately.
- Maintain state counts in real time.
- Display the counts with a LiveCharts2 chart using consistent colors:
  - Optimal — green
  - Acceptable — blue
  - Suboptimal — orange
  - Critical — red
- Ensure chart totals always equal the number of active readings.

## Phase 7 — Component 1 CoreWCF Service

- Self-host the CoreWCF HTTP endpoint inside the Component 1 process.
- Start the service after repositories have loaded.
- Stop it cleanly during application shutdown.
- Implement filtering by species, year, and month.
- Return defensive DTO copies rather than mutable repository collections.
- Validate requests and return empty collections for valid filters with no matches.
- Return a clear service fault for invalid species IDs, months, or years.
- Synchronize access so WCF reads remain safe while the UI modifies collections.
- Show endpoint status in Component 1.

## Phase 8 — Component 2 Retrieval and Mapping

Build a retrieval screen containing:

- Connection status.
- Species selector populated through Component 1.
- Year and month selectors.
- Retrieve/refresh action.
- Loading, empty-result, and error states.

After retrieval:

- Map the response into  
  `Dictionary<string, List<WaterQualityReadingDto>>`.
- Construct the key as `SpeciesId-Name`.
- Replace the existing entry for the same species/month instead of silently duplicating data.
- Present each reading in the required form:
  - `(SpeciesId, HH:mm dd/MM/yyyy) -> [pH, temperature, oxygen]`
- Keep raw DTOs in the ViewModel and use formatting only in the presentation layer.
- If Component 1 is unavailable, load at least three fallback mapped records and visibly mark them as offline sample data.
- Add retry support without restarting Component 2.

## Phase 9 — Statistical Processing

Use a Strategy pattern with one implementation per required method:

- Average pH value by species.
- Minimum oxygen level by species.
- Number of Critical readings by species.

The statistics form will:

- Let the user select one method.
- Process the currently retrieved dictionary.
- Display species ID, species name, method, result, date range, and record count.
- Handle no-data cases without exceptions.
- Format averages consistently while preserving full numeric precision internally.
- Recalculate when the method or retrieved dataset changes.

## Phase 10 — CSV Export

- Export the current statistical result through a standard save dialog.
- Include headers such as:
  - Species ID
  - Species Name
  - Year
  - Month
  - Statistical Method
  - Result
  - Reading Count
- Use UTF-8 encoding and invariant numeric formatting.
- Correctly escape commas, quotes, and line breaks.
- Prevent export when no result exists.
- Report success, cancellation, and file-system errors clearly.

## Phase 11 — UI Composition and Usability

### Component 1

Use navigation or tabs for:

- Species
- Water-quality readings
- State dashboard
- Logs/status

Provide consistent forms, confirmation dialogs, empty states, validation messages, and command availability.

### Component 2

Use sections for:

- Connection and filters
- Retrieved readings
- Statistical method and result
- CSV export

Keep code-behind limited to view-only concerns. All actions must be bound to ViewModel commands.

## Phase 12 — Documentation and UML

Produce project documentation covering:

- Architectural overview and responsibility of each project.
- MVVM structure.
- Repository pattern for persistence.
- Command pattern for undo/redo.
- Strategy pattern for statistics.
- Adapter/mapper responsibility in Component 2.
- CoreWCF communication and hosting choice.
- SOLID principle applications.
- JSON structure, logging, state simulation, validation, and error handling.
- Build and run instructions, including starting Component 1 before Component 2.

Create:

- A UML class diagram showing models, ViewModels, repositories, services, commands, statistical strategies, and WCF contracts.
- A UML sequence diagram for the required scenario:
  1. User selects species, year, and month.
  2. Component 2 ViewModel invokes its client service.
  3. CoreWCF sends the request to Component 1.
  4. Component 1 queries repositories.
  5. Component 1 maps entities to DTOs.
  6. Component 2 receives the response.
  7. The mapper creates the `SpeciesId-Name` dictionary entry.
  8. The ViewModel updates the displayed list.

## Test Plan

- Unit-test model and form validation boundaries.
- Test repository save/load, missing files, empty files, malformed JSON, and enum/date serialization.
- Test every CRUD command and corresponding undo/redo operation.
- Test cascade species deletion and restoration.
- Test search across every property type.
- Test deterministic state order and chart counts.
- Test all three statistical strategies with normal, empty, single-item, and multiple-species datasets.
- Test dictionary key construction and replacement behavior.
- Test CSV escaping, headers, encoding, and numeric formatting.
- Add CoreWCF integration tests for valid requests, invalid filters, empty results, and unavailable service behavior.
- Perform manual acceptance testing of both applications running together.
- Verify that Component 2 can retrieve newly added Component 1 readings without restarting either application.
- Confirm both components load their required fallback data.
- Confirm no business logic is located in WPF code-behind.
- Build the complete solution with warnings reviewed before delivery.

## Assumptions and Locked Defaults

- Existing .NET 10 projects remain in place.
- CoreWCF is used instead of retargeting to .NET Framework.
- JSON is the persistence format.
- Code and UI text are in English.
- `WaterQualityReading.Id` is an internal technical addition needed for reliable CRUD and undo/redo.
- State simulation uses a deterministic five-second cycle.
- Automatic state transitions are logged but are not undoable user actions.
- Component 1 is the authoritative data source.
- Component 2 stores retrieved data only in memory, except for user-requested CSV exports.
- Component 2’s offline seed data is replaced when a live request succeeds.
