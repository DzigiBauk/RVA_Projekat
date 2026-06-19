# Component 1 Implementation Plan

## Scope

Implement only Component 1 of the aquarium monitoring system and the shared
WCF contract required for communication with Component 2.

Component 2 user interface, mapping, statistics, and CSV export are outside
this scope.

## Completed Foundation

- .NET 10 WPF application using MVVM and dependency injection.
- Shared `AquaticSpecies`, `WaterQualityReading`, and `WaterQualityState`
  contracts.
- Shared `IAquariumService` WCF contract.
- In-memory repositories and explicit JSON loading/saving.
- Validation and case-insensitive search for both model types.
- Application Command pattern for CRUD with undo and redo.
- Species and readings interface with DataGrids and validated forms.
- Explicit Load and Save dialogs.
- Required fallback startup data when the default file is missing or empty.
- Display of all required species and reading attributes.
- Disabled-by-default, five-second randomized state simulation.
- Full-state-cycle tracking for every reading.
- Observer notifications for state changes.
- LiveCharts2 dashboard with real-time state counts.
- Timestamped text-file activity logging.
- Logging for CRUD, undo/redo, simulation, startup, explicit persistence, and
  failures.
- Failure-safe logging that cannot terminate the application.

## Phase 11 - CoreWCF Hosting

- Implement the shared `IAquariumService` contract in Component 1.
- Self-host the CoreWCF endpoint inside the WPF process.
- Return copied species and reading lists.
- Filter readings by species, year, and month.
- Validate incoming requests.
- Start the endpoint with Component 1 and stop it during shutdown.
- Display endpoint availability in the interface.
- Keep repositories, validation, persistence, and mutation services private.

## Phase 12 - Testing and Documentation

- Test validation, search, CRUD, cascade deletion, undo, and redo.
- Test explicit JSON load/save and confirm shutdown never saves data.
- Test fallback startup data for missing and empty default files.
- Test that simulation visits every state and never enters undo history.
- Test LiveCharts2 counts after CRUD, loading, undo/redo, and simulation.
- Test logging content and failure handling.
- Test WCF filtering, copied results, invalid requests, and empty results.
- Produce a UML class diagram for Component 1 and shared contracts.
- Produce the required sequence diagram for Component 2 requesting, mapping,
  and displaying Component 1 data.
- Document MVVM, Command, Observer, Repository, CoreWCF, and SOLID usage.

## Locked Decisions

- Code and user interface text are in English.
- JSON persistence is initiated only by explicit user actions.
- No data is saved during shutdown.
- The default startup file is `aquarium-data.json` beside the executable.
- Missing or empty startup data is replaced in memory with three species and
  three readings, but is not written automatically.
- Shared models are used directly as WCF data contracts; separate DTO classes
  are not required.
- Component 1 is the authoritative data source.
