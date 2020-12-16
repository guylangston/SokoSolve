
class serverSideState {
    topElement;
    eventId;
    serverSideUrl;
    leaseId;

    init(leaseId, serverSideUrl) {
        const ctx = this;

        this.leaseId = leaseId;
        this.serverSideUrl = serverSideUrl;

        document.addEventListener('keydown', e => {
            ctx.action("init", e.code);
        });

        this.topElement = document.getElementById("ssc-"+leaseId);
        this.topElement.addEventListener('click', onclick);

        function onclick(e) {
            const inner = ctx.eventId++;
            // var rect = topElement.getBoundingClientRect();
            // console.log(e, rect);

            const x = e.layerX;
            const y = e.layerY;

            $.ajax(`${ctx.serverSideUrl}?verb=click&mouseX=${x}&mouseY=${y}&e=${inner}`)
                .done(function(update) {
                    ctx.process(inner, update);
                });
        }
        
        this.action("init", null);
    }

    showDebug(txt) {
        //console.log(eventId, txt);
        document.getElementById("serverside-debug").innerText = `e${this.eventId}=${txt}`;
    }

    action(verb, key){
        const ctx = this;
        const inner = this.eventId++;
        $.ajax(this.serverSideUrl+"?verb="+verb+"&key="+key+"&e="+inner)
            .done(function(update) {
                ctx.process(inner, update);
            });
    }

    process(inner, update) {
        if (update.text != null){
            this.showDebug(update.text);
        }
        if (update.html != null){
            if (update.target == null){
                this.topElement.innerHTML = update.html;
            }
            else {
                document.getElementById(update.target).innerHTML = update.html;
            }
        }

        if (update.error != null) {
            this.showDebug("ERROR " + update.error);
        }

    }


}




