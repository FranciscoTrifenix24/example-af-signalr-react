using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace af_test {

    /// <summary>
    /// Ejemplo de bus en Azure function, que permitirá poder realizar flujos
    /// </summary>
    public static class ExampleBus {


        /// <summary>
        /// nombre en la cabecera para la autorización.
        /// </summary>
        private const string AUTH_HEADER_NAME = "Authorization";

        /// <summary>
        /// Bearer es normalmente usado en las cabeceras http para autenticación.
        /// </summary>
        private const string BEARER_PREFIX = "Bearer ";
        
        
        /// <summary>
        /// Funcion de Servicebus con signelR
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signalRMessages"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Test-bus")]
        public static async Task RunBus(
            [ServiceBusTrigger("agroqueue", Connection = "ServiceBusConnectionString", IsSessionsEnabled = true)]Message message,
            [SignalR(HubName = "chat")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log) {
            Thread.Sleep(1000);
            log.LogInformation($"----------------------------------------------------------------------------------------");
            log.LogInformation($"Se finite {Encoding.UTF8.GetString(message.Body)} and session : {message.SessionId}");
            log.LogInformation($"----------------------------------------------------------------------------------------");
            await signalRMessages.AddAsync(
                new SignalRMessage {
                    Target = "send",
                    Arguments = new[] { $"Termino {message.SessionId} ejemplo para el cristian" }
                });
        }


        /// <summary>
        /// Función para abrir conexión con signalr
        /// </summary>
        /// <param name="req">requerimiento normal http</param>
        /// <param name="connectionInfo">conexión de signalr</param>
        /// <returns></returns>
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]HttpRequest req,
            [SignalRConnectionInfo(HubName = "chat")]SignalRConnectionInfo connectionInfo) {
            return connectionInfo;
        }

        /// <summary>
        /// Procesa mensaje de cliente
        /// </summary>
        /// <param name="req"></param>
        /// <param name="binder"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("MessagesNegotiateBinding")]
        public static IActionResult NegotiatBinding(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1.0/messages/binding/negotiate")] HttpRequest req,
            IBinder binder,
            ILogger log) {
            if (req.Headers.ContainsKey(AUTH_HEADER_NAME) && req.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX)) {
                var token = req.Headers["Authorization"].ToString().Substring(BEARER_PREFIX.Length);
                log.LogInformation("with binding " + token);
                
                var userId = "userIdExctractedFromToken"; // needs real impl...
                var connectionInfo = binder.Bind<SignalRConnectionInfo>(new SignalRConnectionInfoAttribute { HubName = "chat", UserId = userId });
                log.LogInformation("negotiated " + connectionInfo);
                //https://gist.github.com/ErikAndreas/72c94a0c8a9e6e632f44522c41be8ee7
                // connectionInfo contains an access key token with a name identifier claim set to the authenticated user
                return new OkObjectResult(connectionInfo);                
            }
            else
                return new BadRequestObjectResult("No access token submitted.");
        }

        /// <summary>
        /// envía mensaje a clientes
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signalRMessages"></param>
        /// <returns></returns>
        [FunctionName("SendMessage")]
        public static Task SendMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]object message,
        [SignalR(HubName = "chat")]IAsyncCollector<SignalRMessage> signalRMessages) {
            return signalRMessages.AddAsync(
                new SignalRMessage {
                    Target = "send",
                    Arguments = new[] { "mensaje desde http" }
                });
        }

    }

}