// Simple JavaScript for Inventory Management System

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    console.log('Inventory Management System loaded');
    
    // Setup delete confirmations
    document.querySelectorAll('[data-confirm-delete]').forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item?')) {
                e.preventDefault();
            }
        });
    });
});
