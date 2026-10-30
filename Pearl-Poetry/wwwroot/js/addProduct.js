<script>
setTimeout(function () {
    const alert = document.querySelector('.alert-success-custom');

    if (alert) {
        alert.style.transition = "0.5s";
        alert.style.opacity = "0";

        setTimeout(() => {
            alert.remove();
        }, 500);
    }
}, 3000);
</script>