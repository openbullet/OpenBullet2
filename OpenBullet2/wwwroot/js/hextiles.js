// Credits: https://codepen.io/towc/pens

var opts = {
      
      side: 35,
      picksParTick: 2,
      baseTime: 80,
      addedTime: 20,
      
			colors: [ 'rgba(28,28,28,alp)', 'rgba(32,32,32,alp)', 'rgba(38,38,38,alp)', 'rgba(42,42,42,alp)' ],
      addedAlpha: 20,
      strokeColor: 'rgb(0,0,0)',
      
      hueSpeed: .4,
      repaintAlpha: 1
    },
    
    difX = Math.sqrt( 3 ) * opts.side / 2, // height of a equilateral triangle 
    difY = opts.side * 3 / 2, // side of a triangle ( because it goes down to a vertex ) then half a side of the triangle in the hex below: s + s/2 = s*3/2
    rad = Math.PI / 6, // TAU / 6 = PI / 3 I thought, but apparently this way works better
    cos = Math.cos( rad ) * opts.side,
    sin = Math.sin( rad ) * opts.side,
    
    hexs = [],
    tick = 0;

var w, h, sum, ctx;

function loop(){
  
  window.requestAnimationFrame( loop );
  
  tick += opts.hueSpeed;
  
  ctx.shadowBlur = 0;
  ctx.fillStyle = 'rgba(22,22,22,alp)'.replace( 'alp', opts.repaintAlpha );
  ctx.fillRect( 0, 0, w, h );
  
  for( var i = 0; i < opts.picksParTick; ++i )
    hexs[ ( Math.random() * hexs.length ) |0 ].pick();
  
  hexs.map( function( hex ){ hex.step(); } );
}

function Hex( x, y ){
  
  this.x = x;
  this.y = y;
  this.sum = this.x + this.y;
  this.picked = false;
  this.time = 0;
  this.targetTime = 0;
  
  this.xs = [ this.x + cos, this.x, this.x - cos, this.x - cos, this.x, this.x + cos ];
  this.ys = [ this.y - sin, this.y - opts.side, this.y - sin, this.y + sin, this.y + opts.side, this.y + sin ];
}

Hex.prototype.pick = function(){
  
	this.color = opts.colors[ ( Math.random() * opts.colors.length ) |0 ];
  this.picked = true;
  this.time = this.time || 0;
  this.targetTime = this.targetTime || ( opts.baseTime + opts.addedTime * Math.random() ) |0;
}

Hex.prototype.step = function(){
  
  var prop = this.time / this.targetTime;
  
  ctx.beginPath();
  ctx.moveTo( this.xs[0], this.ys[0] );
  for( var i = 1; i < this.xs.length; ++i )
    ctx.lineTo( this.xs[i], this.ys[i] );
  ctx.lineTo( this.xs[0], this.ys[0] );
  
  if( this.picked ){
    
    ++this.time;
    
    if( this.time >= this.targetTime ){
      
      this.time = 0;
      this.targetTime = 0;
      this.picked = false;
    }
    
    ctx.fillStyle = ctx.shadowColor = this.color.replace( 'alp', Math.sin( prop * Math.PI ) );
    ctx.fill();
  } else {
    
    ctx.strokeStyle = ctx.shadowColor = opts.strokeColor;
    ctx.stroke();
  }
}

function startHexTiles(){
  var c = document.getElementById('setup-canvas');

  this.w = c.width = window.innerWidth;
  this.h = c.height = window.innerHeight;
  this.sum = w + h;
  this.ctx = c.getContext('2d');

  for (var x = 0; x < w; x += difX * 2) {
    var i = 0;

    for (var y = 0; y < h; y += difY) {
      ++i;
      hexs.push(new Hex(x + difX * (i % 2), y));
    }
  }

  loop();

  window.addEventListener( 'resize', function(){
  
    w = c.width = window.innerWidth;
    h = c.height = window.innerHeight;
    sum = w + h;
    
    hexs.length = 0;
    for( var x = 0; x < w; x += difX*2 ){
      var i = 0;
  
      for( var y = 0; y < h; y += difY ){
        ++i;
        hexs.push( new Hex( x + difX * ( i % 2 ), y ) );
  
      }
    }
  })
}

