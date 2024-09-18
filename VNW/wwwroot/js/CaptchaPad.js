//try to set java script in partial view
console.log('js in pv 2')
const btn_0 = document.getElementById('btn_0');
const btn_1 = document.getElementById('btn_1');
const btn_2 = document.getElementById('btn_2');
const btn_3 = document.getElementById('btn_3');
const btn_4 = document.getElementById('btn_4');
const btn_5 = document.getElementById('btn_5');
const btn_6 = document.getElementById('btn_6');
const btn_7 = document.getElementById('btn_7');
const btn_8 = document.getElementById('btn_8');
const btn_9 = document.getElementById('btn_9');
const btn_del = document.getElementById('btn_del');
const btn_clear = document.getElementById('btn_clear');

btn_0.addEventListener('click', function () {
    Captcha.value = Captcha.value + '0';
});
btn_1.addEventListener('click', function () {
    Captcha.value = Captcha.value + '1';
});
btn_2.addEventListener('click', function () {
    Captcha.value = Captcha.value + '2';
});
btn_3.addEventListener('click', function () {
    Captcha.value = Captcha.value + '3';
});
btn_4.addEventListener('click', function () {
    Captcha.value = Captcha.value + '4';
});
btn_5.addEventListener('click', function () {
    Captcha.value = Captcha.value + '5';
});
btn_6.addEventListener('click', function () {
    Captcha.value = Captcha.value + '6';
});
btn_7.addEventListener('click', function () {
    Captcha.value = Captcha.value + '7';
});
btn_8.addEventListener('click', function () {
    Captcha.value = Captcha.value + '8';
});
btn_9.addEventListener('click', function () {
    Captcha.value = Captcha.value + '9';
});
btn_clear.addEventListener('click', function () {
    Captcha.value = '';
});
btn_del.addEventListener('click', function () {
    var len = Captcha.value.length;
    //console.log('len: ' + len);
    if (len => 1) {
        var temp = Captcha.value.substring(0, len - 1);
        Captcha.value = temp;
    }
});