## Purpose

Provides an ASP.NET Core MVC frontend web interface adopting the retro-arcade visual format of Layout.fig with dynamic asynchronous API consumption and skeleton loading, adhering to 02 ASP.NET Core MVC - Desarrollo.pptx.

## Requirements

### Requirement: Landing and Hero Display
The system SHALL display a hero landing section faithful to the format in Layout.fig, featuring the retro badge "SABOR INTERESTELAR DESDE 1982", the heading "PIZZA PLANETA", thematic cosmic copy, interactive arcade-styled buttons ("VER PIZZAS" and "SOPORTE DE CONTROL"), and the illustrated pizza planet graphic with glowing neon rings.

#### Scenario: User visits the homepage
- **WHEN** a user opens the home page at `/` or `/Home/Index`
- **THEN** the hero section SHALL be displayed with the retro badge, heading, intro text, action buttons, and glowing pizza planet artwork.

### Requirement: Hazard Caution Marquee
The system SHALL display a retro hazard accent ribbon across the viewport separating the hero section from the catalog section, displaying repeating warning text "WARNING: DANGER OF DELICIOUSNESS" with high-visibility contrast styling.

#### Scenario: Viewing the transition between hero and catalog
- **WHEN** the page is loaded
- **THEN** the caution ribbon SHALL span horizontally with repeating hazard copy between sections.

### Requirement: Asynchronous API Data Ingestion
The system SHALL query the PizzaPlanetaApi asynchronously via non-blocking HTTP GET methods (`async/await`) to retrieve dynamic pizza records (`PizzaDto`), handling connection states and errors gracefully without blocking thread execution.

#### Scenario: Fetching pizzas asynchronously
- **WHEN** the catalog action requests data from PizzaPlanetaApi
- **THEN** the request SHALL be performed using asynchronous HTTP calls (`async/await`) to the `/pizzas` endpoint without thread-blocking operations.

### Requirement: Dynamic Pizza Card Component
The system SHALL provide a reusable Razor component or partial view that renders each pizza received from the API into the visual card format specified in Layout.fig, featuring a themed neon frame, dynamic title, description, and "COSMIC PRICE" badge.

#### Scenario: Rendering pizza items dynamically from API
- **WHEN** the catalog receives pizza items from the API
- **THEN** each item SHALL be rendered through the pizza card component formatted according to the Figma card specifications.

### Requirement: Skeleton Loading Display
The system SHALL display animated skeleton loading card placeholders matching the dimensions and grid layout of the pizza cards while data is being retrieved asynchronously or pending response, with glowing neon pulse animation.

#### Scenario: Viewing catalog during asynchronous data retrieval
- **WHEN** pizza data is loading or being queried from the API
- **THEN** the system SHALL display skeleton cards with glowing pulse animations prior to displaying the populated cards.

### Requirement: HUD Catalog Filtering and Search
The system SHALL provide interactive HUD controls styled as retro telemetry receptors: a search input labeled `[ BUSCADOR RECEPTOR ]` to filter pizzas by keyword, and an ordering dropdown labeled `[ TELEMETRÍA DE COSTOS ]` to sort pizzas by price.

#### Scenario: Filtering catalog by search query
- **WHEN** a user enters a query in the search receptor input
- **THEN** the catalog SHALL display only the pizzas whose names or descriptions match the search criteria.

#### Scenario: Ordering catalog by price
- **WHEN** a user selects an ordering option from the telemetry selector
- **THEN** the displayed pizzas SHALL be sorted according to the selected price order.

### Requirement: MVC Architecture and Tag Helper Compliance
The frontend application SHALL implement the Model-View-Controller (MVC) architectural pattern in accordance with 02 ASP.NET Core MVC - Desarrollo.pptx. Controllers SHALL handle requests using explicit asynchronous HTTP action methods (`[HttpGet]`), return strongly-typed ViewModels to Razor views, and Razor views SHALL utilize ASP.NET Core Tag Helpers (`asp-controller`, `asp-action`, `asp-for`) and Razor flow control statements (`@foreach`, `@if`).

#### Scenario: Rendering controller action with strongly typed model
- **WHEN** a request arrives at the home controller index action
- **THEN** the action method SHALL fetch catalog data asynchronously via a dedicated API service and pass a strongly-typed ViewModel to the Razor view.

### Requirement: Retro Space Footer and Arcade Controls
The system SHALL display a footer containing the branding and copyright notice "© 1982-2026 PIZZA PLANETA. ALL PROTOCOLS ENFORCED." and the arcade-styled prompt "INSERT COIN TO PLAY [ 0.25$ ]".

#### Scenario: Viewing the application footer
- **WHEN** navigating to the bottom of the page
- **THEN** the footer SHALL display the protocol enforcement notice on the left and the insert coin prompt on the right.
