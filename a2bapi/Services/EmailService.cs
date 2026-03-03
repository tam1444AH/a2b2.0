using a2bapi.Dtos.Flights;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using a2bapi.Dtos.Hotels;

namespace a2bapi.Services
{
    public interface IEmailService
    {
        Task SendFlightBookingConfirmationAsync(string userEmail, BookFlightRequest request);
        Task SendHotelBookingConfirmationAsync(string userEmail, BookHotelRequest request);
    }
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendFlightBookingConfirmationAsync(string userEmail, BookFlightRequest request)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password) || string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                throw new InvalidOperationException("SMTP settings are not configured properly.");
            }

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"Booking Confirmation for Flight: {request.FlightName}";

            var builder = new BodyBuilder
            {
                TextBody = $@"
Dear fellow traveler,

Thank you for booking your flight with A2B Travel!

Here are your booking details:
Flight: {request.FlightName}
Date: {request.FlightDate}
Departure: {request.DepartureIata} at {request.DepartureTime}
Arrival: {request.ArrivalIata} at {request.ArrivalTime}
Tickets: {request.NumTickets}
Total Cost: ${request.TotalCost}

Safe travels,
The A2B Team
",
                HtmlBody = $@"
<strong>Dear fellow traveler,</strong><br/><br/>
Thank you for booking your flight with <strong>A2B Travel</strong>!<br/><br/>

<strong>Here are your booking details:</strong>
<ul>
    <li>Flight: {request.FlightName}</li>
    <li>Date: {request.FlightDate}</li>
    <li>Departure: {request.DepartureIata} at {request.DepartureTime}</li>
    <li>Arrival: {request.ArrivalIata} at {request.ArrivalTime}</li>
    <li>Tickets: {request.NumTickets}</li>
    <li>Total Cost: <strong>${request.TotalCost}</strong></li>
</ul>

Safe travels,<br/>
<strong>The A2B Team</strong>
"
            };

            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendHotelBookingConfirmationAsync(string userEmail, BookHotelRequest request)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password) || string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                throw new InvalidOperationException("SMTP settings are not configured properly.");
            }

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"Booking Confirmation for Hotel: {request.HotelName}";

            var builder = new BodyBuilder
            {
                TextBody = $@"
Dear fellow traveler,

Thank you for booking your hotel with A2B Travel!

Here are your booking details:
Hotel: {request.HotelName}
Distance: {request.HotelDistance} MI
IATA Code: {request.HotelIataCode}
Country Code: {request.HotelCountryCode}
Rating: {request.HotelRating} stars
Number of nights: {request.NumNights}
Total Cost: {request.TotalCost}

Safe travels,
The A2B Team
",
                HtmlBody = $@"
<strong>Dear fellow traveler,</strong><br/><br/>
Thank you for booking your hotel with <strong>A2B Travel</strong>!<br/><br/>

<strong>Here are your booking details:</strong>
<ul>
    <li>Hotel: {request.HotelName}</li>
    <li>Distance: {request.HotelDistance} miles</li>
    <li>IATA Code: {request.HotelIataCode}</li>
    <li>Country Code: {request.HotelCountryCode}</li>
    <li>Rating: {request.HotelRating} stars</li>
    <li>Number of nights: {request.NumNights}</li>
    <li>Total Cost: <strong>${request.TotalCost}<strong/></li>
</ul>

Safe travels,<br/>
<strong>The A2B Team</strong>
"
            };

            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


    }
}
