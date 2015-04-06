using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SignalR.Hubs
{
    public class ProgressBarHub : Hub
    {
        private readonly IServicoPedido servicoPedido;

        public ProgressBarHub()
        {
            this.servicoPedido = new ServicoPedido();
            this.servicoPedido.OrderProcessed += servicoPedido_OrderProcessed;
        }

        void servicoPedido_OrderProcessed(object sender, PedidoQuantidadeEventArgs e)
        {
            var percentual = ((decimal)e.QuantidadeAtual / (decimal)e.QuantidadeTotal) * 100M;

            Thread.Sleep(250);

            this.SendStatus(e.QuantidadeAtual, e.QuantidadeTotal);
        }

        public void SendStatus(int qtdAtual, int qtdTotal)
        {
            Clients.Caller.sendStatus(qtdAtual, qtdTotal);
        }

        public void Process()
        {
            this.servicoPedido.Processar();
        }
    }

    public class PedidoEventArgs : EventArgs
    {
        public Pedido Pedido { get; set; }
    }

    public class PedidoQuantidadeEventArgs : EventArgs
    {
        public Pedido Pedido { get; set; }
        public int QuantidadeAtual { get; set; }
        public int QuantidadeTotal { get; set; }
    }

    public interface IServicoPedido
    {
        event EventHandler<PedidoEventArgs> StatusChanged;
        event EventHandler<PedidoQuantidadeEventArgs> OrderProcessed;
        void Criar();
        void Atualizar();
        void Processar();
    }

    public class ServicoPedido : IServicoPedido
    {
        List<Pedido> pedidos = new List<Pedido>();

        public event EventHandler<PedidoEventArgs> StatusChanged;
        public event EventHandler<PedidoQuantidadeEventArgs> OrderProcessed;

        protected void StatusOnChanged(PedidoEventArgs e)
        {
            if (StatusChanged != null)
            {
                StatusChanged.Invoke(this, e);
            }
        }

        protected void OrderOnProcessed(PedidoQuantidadeEventArgs e)
        {
            if (OrderProcessed != null)
            {
                OrderProcessed.Invoke(this, e);
            }
        }

        public void Criar()
        {
            var pedido = new Pedido()
            {
                Numero = 1,
                Status = StatusPedido.EmAnalise
            };

            pedidos.Add(pedido);
        }

        public void Atualizar()
        {
            var pedido = this.pedidos.SingleOrDefault(p => p.Numero == 1);

            pedido.Status = StatusPedido.PendenteEnvio;

            this.StatusOnChanged(new PedidoEventArgs { Pedido = pedido });
        }

        public void Processar()
        {
            var pedidos = new List<Pedido>();

            for (int i = 0; i < 100; i++)
            {
                var pedido = new Pedido { Numero = i, Status = StatusPedido.Enviado };

                pedidos.Add(pedido);                
            }

            for (int i = 0; i < 100; i++)
            {                
                this.StatusOnChanged(new PedidoEventArgs { Pedido = pedidos[i] });
                this.OrderOnProcessed(new PedidoQuantidadeEventArgs { Pedido = pedidos[i], QuantidadeAtual = i + 1, QuantidadeTotal = pedidos.Count });
            }
        }
    }

    public class Pedido
    {
        public int Numero { get; set; }
        public StatusPedido Status { get; set; }
    }

    public enum StatusPedido
    {
        EmAnalise,
        PendenteEnvio,
        Enviado,
        Entregue
    } 
}