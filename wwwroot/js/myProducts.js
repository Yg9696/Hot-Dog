<script>
    function notifyMe() {
            var productId = document.querySelector("#notifyMeForm input[name='productId']").value;

    $.ajax({
        url: '/Products/NotifyMe', // Update the URL to match your controller and action
    type: 'POST',
    data: {productId: productId },
    success: function (response) {
        alert('Notification sent successfully!');
                    // Optionally, you can update the UI based on the response
                },
    error: function (xhr, status, error) {
        console.error(xhr.responseText);
    alert('Failed to send notification!');
                    // Optionally, handle errors or update UI
                }
            });
        }
</script>
