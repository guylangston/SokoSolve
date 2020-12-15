function showDebug(txt) {
    //console.log(eventId, txt);
    document.getElementById("serverside-debug").innerText = `e${eventId}=${txt}`;
}

var topElement;
var eventId = 0;


function serverSideStateInit(leaseId, serverSideUrl){

    document.addEventListener('keydown', e => {
        var inner = eventId++;
        $.ajax(serverSideUrl+"?verb=key&key="+e.code+"&e="+inner)
            .done(function(update) {
                process(inner, update);
            });
    });
    
    topElement = document.getElementById("ssc-"+leaseId);
    topElement.addEventListener('click', onclick);
    
   
    function onclick(e) {
        var inner = eventId++;
        // var rect = topElement.getBoundingClientRect();
        // console.log(e, rect);
        
        var x = e.layerX;
        var y = e.layerY;
        
        $.ajax(`${serverSideUrl}?verb=click&mouseX=${x}&mouseY=${y}&e=${inner}`)
            .done(function(update) {
                process(inner, update);
            });
    }
    
    

    function process(inner, update) {
        if (update.text != null){
            showDebug(update.text);
        }
        if (update.html != null){
            if (update.target == null){
                topElement.innerHTML = update.html;    
            }
            else {
                document.getElementById(update.target).innerHTML = update.html;
            }
            
        }
        
        if (update.error != null) {
            showDebug("ERROR " + update.error);
        }
        
    }
    
    $.ajax(serverSideUrl+"?verb=init")
        .done(function(update) {
            process(null, update);
        });

   
}

