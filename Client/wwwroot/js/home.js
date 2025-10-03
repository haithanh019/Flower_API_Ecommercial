// Homepage JavaScript functionality - Fixed version
document.addEventListener('DOMContentLoaded', function () {

    // Initialize homepage features
    initScrollAnimations();
    initProductCards();
    initCategoryCards();
    initTestimonialSlider();
    initAddToCart();
    initSmoothScroll();
    initHeroAnimations();

    // Scroll animations
    function initScrollAnimations() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in');
                }
            });
        }, observerOptions);

        // Observe elements for animation
        document.querySelectorAll('.category-card, .product-card, .feature-card, .testimonial-card').forEach(el => {
            observer.observe(el);
        });
    }

    // Product card interactions
    function initProductCards() {
        const productCards = document.querySelectorAll('.product-card');

        productCards.forEach(card => {
            const image = card.querySelector('.product-image img');

            // Image error handling
            if (image) {
                image.addEventListener('error', function () {
                    this.src = '/images/flowers/placeholder.jpg';
                    this.alt = 'Product image not available';
                });

                // Lazy loading effect
                if (image.complete) {
                    card.classList.add('loaded');
                } else {
                    image.addEventListener('load', () => {
                        card.classList.add('loaded');
                    });
                }
            }

            // Card hover effects
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-8px) scale(1.02)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0) scale(1)';
            });
        });
    }

    // Category card interactions
    function initCategoryCards() {
        const categoryCards = document.querySelectorAll('.category-card');

        categoryCards.forEach(card => {
            card.addEventListener('click', function (e) {
                if (e.target.tagName !== 'A') {
                    const link = this.querySelector('a');
                    if (link) {
                        link.click();
                    }
                }
            });

            // Add ripple effect
            card.addEventListener('click', function (e) {
                createRipple(e, this);
            });
        });
    }

    // Create ripple effect
    function createRipple(event, element) {
        const ripple = document.createElement('span');
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        ripple.style.cssText = `
            position: absolute;
            border-radius: 50%;
            background: rgba(231, 76, 124, 0.3);
            transform: scale(0);
            animation: ripple 0.6s linear;
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            pointer-events: none;
        `;

        element.style.position = 'relative';
        element.style.overflow = 'hidden';
        element.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    // Add to cart functionality
    function initAddToCart() {
        const addToCartButtons = document.querySelectorAll('.add-to-cart');

        addToCartButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                const productId = this.dataset.productId;

                // Add loading state
                const originalText = this.innerHTML;
                this.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang thêm...';
                this.disabled = true;

                // Simulate API call
                setTimeout(() => {
                    addToCartAPI(productId)
                        .then(() => {
                            this.innerHTML = '<i class="fas fa-check me-1"></i>Đã thêm!';
                            this.classList.add('btn-success');

                            // Update cart counter
                            updateCartCounter();

                            // Show success message
                            showNotification('Đã thêm sản phẩm vào giỏ hàng!', 'success');

                            // Reset button after 2 seconds
                            setTimeout(() => {
                                this.innerHTML = originalText;
                                this.classList.remove('btn-success');
                                this.disabled = false;
                            }, 2000);
                        })
                        .catch(() => {
                            this.innerHTML = originalText;
                            this.disabled = false;
                            showNotification('Có lỗi xảy ra, vui lòng thử lại!', 'error');
                        });
                }, 500);
            });
        });
    }

    // Mock API call for adding to cart
    async function addToCartAPI(productId) {
        const response = await fetch('/Cart?handler=Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                productId: productId,
                quantity: 1
            })
        });

        if (!response.ok) {
            throw new Error('Failed to add to cart');
        }

        return response.json();
    }

    // Update cart counter
    async function updateCartCounter() {
        try {
            const response = await fetch('/Cart?handler=Count');
            const data = await response.json();

            if (data.ok) {
                const counter = document.querySelector('.cart-counter');
                if (counter) {
                    counter.textContent = data.count;
                    counter.style.display = data.count > 0 ? 'inline' : 'none';

                    // Animate counter
                    counter.classList.add('animate-pulse');
                    setTimeout(() => {
                        counter.classList.remove('animate-pulse');
                    }, 1000);
                }
            }
        } catch (error) {
            console.error('Failed to update cart counter:', error);
        }
    }

    // Show notification
    function showNotification(message, type = 'info') {
        // Remove existing notifications
        document.querySelectorAll('.toast-notification').forEach(toast => {
            toast.remove();
        });

        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
                ${message}
            </div>
            <button class="toast-close" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        `;

        document.body.appendChild(toast);

        // Auto remove after 3 seconds
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, 3000);
    }

    // Smooth scroll for anchor links
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));

                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Hero animations
    function initHeroAnimations() {
        const heroTitle = document.querySelector('.hero-title');
        const heroSubtitle = document.querySelector('.hero-subtitle');
        const heroButtons = document.querySelector('.hero-buttons');
        const heroStats = document.querySelector('.hero-stats');

        // Staggered animation
        if (heroTitle) {
            setTimeout(() => heroTitle.classList.add('animate-fade-in'), 100);
        }
        if (heroSubtitle) {
            setTimeout(() => heroSubtitle.classList.add('animate-fade-in'), 300);
        }
        if (heroButtons) {
            setTimeout(() => heroButtons.classList.add('animate-fade-in'), 500);
        }
        if (heroStats) {
            setTimeout(() => heroStats.classList.add('animate-fade-in'), 700);
        }

        // Floating elements animation
        const floatingElements = document.querySelectorAll('.floating-element');
        floatingElements.forEach((element, index) => {
            element.style.animationDelay = `${index * 2}s`;
        });
    }

    // Testimonial slider (simple version)
    function initTestimonialSlider() {
        const testimonials = document.querySelectorAll('.testimonial-card');
        let currentIndex = 0;

        if (testimonials.length <= 3) return; // No need to slide if 3 or fewer testimonials

        function showTestimonials() {
            testimonials.forEach((testimonial, index) => {
                testimonial.style.display = index >= currentIndex && index < currentIndex + 3 ? 'block' : 'none';
            });
        }

        function nextSlide() {
            currentIndex = (currentIndex + 3) % testimonials.length;
            showTestimonials();
        }

        // Auto-slide every 5 seconds
        setInterval(nextSlide, 5000);

        // Initialize
        showTestimonials();
    }

    // Parallax effect for hero section - FIXED: Use scrollY instead of pageYOffset
    function initParallax() {
        const hero = document.querySelector('.hero-section');

        if (hero) {
            window.addEventListener('scroll', () => {
                const scrolled = window.scrollY; // FIXED: Changed from pageYOffset to scrollY
                const parallax = scrolled * 0.5;

                hero.style.transform = `translateY(${parallax}px)`;
            });
        }
    }

    // Initialize parallax on larger screens only
    if (window.innerWidth > 768) {
        initParallax();
    }

    // Handle window resize
    window.addEventListener('resize', () => {
        // Reinitialize parallax if needed
        if (window.innerWidth > 768) {
            initParallax();
        }
    });

    // Improved scroll performance with throttling
    function throttle(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Apply throttling to scroll events for better performance
    const throttledScrollHandler = throttle(() => {
        // Any scroll-based animations or calculations can go here
        updateScrollProgress();
    }, 16); // ~60fps

    function updateScrollProgress() {
        const scrollTop = window.scrollY;
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        const scrollPercent = (scrollTop / docHeight) * 100;

        // Update any scroll progress indicators if needed
        const progressBar = document.querySelector('.scroll-progress');
        if (progressBar) {
            progressBar.style.width = `${scrollPercent}%`;
        }
    }

    window.addEventListener('scroll', throttledScrollHandler);
});

// Add CSS for animations and toast notifications
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }
    
    .toast-notification {
        position: fixed;
        top: 20px;
        right: 20px;
        background: white;
        border-radius: 8px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.15);
        padding: 16px 20px;
        display: flex;
        align-items: center;
        justify-content: space-between;
        min-width: 300px;
        z-index: 9999;
        animation: slideIn 0.3s ease-out;
    }
    
    .toast-success {
        border-left: 4px solid #2ecc71;
    }
    
    .toast-error {
        border-left: 4px solid #e74c7c;
    }
    
    .toast-content {
        display: flex;
        align-items: center;
        color: #2c3e50;
        font-weight: 500;
    }
    
    .toast-close {
        background: none;
        border: none;
        color: #7f8c8d;
        cursor: pointer;
        padding: 0;
        margin-left: 15px;
        font-size: 14px;
    }
    
    .toast-close:hover {
        color: #2c3e50;
    }
    
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    .product-card {
        transition: transform 0.3s ease, box-shadow 0.3s ease;
    }
    
    .product-card.loaded {
        opacity: 1;
    }
    
    .cart-counter {
        transition: all 0.3s ease;
    }
    
    /* Scroll progress bar */
    .scroll-progress {
        position: fixed;
        top: 0;
        left: 0;
        height: 3px;
        background: linear-gradient(90deg, #e74c7c, #f39c12);
        z-index: 10000;
        transition: width 0.3s ease;
    }
    
    /* Loading skeleton for product cards */
    .product-card.loading {
        background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
        background-size: 200% 100%;
        animation: loading 1.5s infinite;
    }
    
    @keyframes loading {
        0% {
            background-position: 200% 0;
        }
        100% {
            background-position: -200% 0;
        }
    }
    
    /* Improved accessibility */
    @media (prefers-reduced-motion: reduce) {
        * {
            animation-duration: 0.01ms !important;
            animation-iteration-count: 1 !important;
            transition-duration: 0.01ms !important;
        }
    }
    
    /* Focus states for better keyboard navigation */
    .category-card:focus,
    .product-card:focus,
    .btn:focus {
        outline: 2px solid #e74c7c;
        outline-offset: 2px;
    }
`;
document.head.appendChild(style);