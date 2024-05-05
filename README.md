# Ticket/Cruise Booking System

## Description
This project simulates a ticket/cruise booking system with clients (ticket agents) and servers (cruises) using multithreading and event-driven programming. It involves dynamic pricing of tickets, order processing, and communication between agents and cruises through a multi-cell buffer.

## Features
- **Dynamic Pricing**: Cruises dynamically calculate ticket prices based on various factors, with the ability to emit price-cut events.
- **Order Processing**: Ticket agents evaluate prices and place orders, which are processed by the cruise, including credit card validation and total amount calculation.
- **Multi-Cell Buffer**: Communication between agents and cruises is facilitated through a multi-cell buffer, ensuring thread safety and data integrity.
- **Event-Driven**: Agents subscribe to price-cut events and receive notifications to adjust orders accordingly.

## Components
1. **Cruise**: Handles pricing and emits price-cut events. Terminates after a specified number of price cuts.
2. **PricingModel**: Determines ticket prices based on various parameters, ensuring variability within iterations.
3. **OrderProcessing**: Processes orders, including credit card validation and total charge calculation.
4. **TicketAgent**: Represents clients, subscribes to price-cut events, and places orders accordingly.
5. **OrderClass**: Contains order details such as sender/receiver IDs, card number, quantity, and unit price.
6. **MultiCellBuffer**: Facilitates communication between agents and cruises, managing data cells and access permissions.
7. **Bank Service** (for team projects): Manages credit card validation and account transactions.
8. **Main**: Initializes necessary components, creates threads, and starts execution.

## Additional Tasks (For Team Projects)
- **Multiple Suppliers**: Implement multiple cruises with complex price models.
- **Bank Service**: Manage credit card validation and transactions for order payments.

## Usage
- Clone the repository.
- Compile and execute the program.
- View console output for order processing and system interactions.

## Contributors
- **[Dimitar Atanassov]**

## License
This project is licensed under the [MIT License](LICENSE).
