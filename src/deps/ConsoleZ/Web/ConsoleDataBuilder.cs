using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ.Web
{
    public class ConsoleDataBuilder
    {
        private string urlTemplate;
        
        IConsoleRenderer lineRenderer = new HtmlConsoleRenderer();

        public ConsoleDataBuilder(string urlTemplate)
        {
            this.urlTemplate = urlTemplate;
        }

        public string DefaultRenderer(IConsole cons)
        {
            if (cons is VirtualConsole vCons)
            {
                var sb = new StringBuilder();
                var buffer = vCons.GetTextLines();
                for (int i = vCons.DisplayStart; i < buffer.Count; i++)
                {
                    sb.AppendLine(lineRenderer.RenderLine(cons, i, buffer[i]));
                }

                return sb.ToString();
            }
            
            return $"Not Supported: {cons.GetType().Name}";
        }


        public ConsoleData ToDto(IConsole cons)
        {
            if (cons == null) throw new ArgumentNullException(nameof(cons));
            if (cons.Handle == null) throw new ArgumentNullException(nameof(cons.Handle));

            var dto = new ConsoleData()
            {
                Handle = cons.Handle,
                Width = cons.Width,
                Height = cons.Height,
                Version = cons.Version,
                
                UpdateUrl = string.Format(urlTemplate, cons.Handle),
                HtmlContent = DefaultRenderer(cons)
            };

            if (cons is IConsoleWithProps consProps)
            {
                if (consProps.TryGetProp("IsActive", out var active))
                {
                    dto.IsActive = bool.Parse(active);
                }

                if (consProps.TryGetProp("DoneUrl", out var done)) dto.DoneUrl = done;
                if (consProps.TryGetProp("BackUrl", out var back)) dto.BackUrl = back;
                if (consProps.TryGetProp("CancelUrl", out var cancel)) dto.CancelUrl = cancel;

                dto.Props = new Dictionary<string, string>()
                {
                    {"title", consProps.Title}
                };
                
                dto.Title = consProps.Title;
            }

            return dto;
        }

        public Task RunAsync(IConsoleWithProps cons, Action<IConsoleWithProps> action)
        {
            return Task.Run(() =>
            {
                try
                {
                    cons.SetProp("IsActive", true.ToString());
                    action(cons);
                }
                catch (Exception e)
                {
                    cons.WriteLine(e.ToString());
                    cons.SetProp("Error", e.GetType().Name);
                }
                finally
                {
                    cons.SetProp("IsActive", false.ToString());
                }

            });
        }
    }
}