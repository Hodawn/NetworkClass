let express = require('express');       //express 모듈을 가져온다.
let app = express();                    //express를 app 이름으로 정의하고 사용한다

app.get('/', function(req, res){        //기본 라우터에서 Hello world를 반환한다.
    res.send('Hello World');
});

app.get('/about',function(req, res){
    res.send('about World');
});
app.get('/ScoreData',function(req, res){
    res.send('ScoreData World');
});
app.get('/ItemData',function(req, res){
    res.send('ItemData World');
});
app.listen(3000, function(){                //3000포트에서 입력을 대기 한다.
    console.log('Example app listening on port 3000');
})