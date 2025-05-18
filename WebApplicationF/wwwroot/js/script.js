const apiBaseUrl = "";
const cart = [];

function addToCart(product, quantity) {
  const existing = cart.find(item => item.id === product.id);
  if (existing) {
    existing.quantity += quantity;
  } else {
    cart.push({ ...product, quantity });
  }
  updateCartDisplay();
  updateCartCount();
}

function removeFromCart(index) {
  cart.splice(index, 1);
  updateCartDisplay();
  updateCartCount();
}

function updateCartCount() {
  const badge = document.getElementById("cart-count");
  const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
  badge.textContent = totalItems;
  badge.style.display = totalItems > 0 ? "block" : "none";
}

function updateCartDisplay() {
  const content = document.getElementById("cart-content");

  if (cart.length === 0) {
    content.innerHTML = "<p>Your cart is currently empty.</p>";
    return;
  }

  let html = "<ul>";
  cart.forEach((item, index) => {
    const total = ((item.discountedPrice ?? item.price) * item.quantity).toFixed(2);
    html += `
      <li style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem;">
        <span>${item.name} x${item.quantity} – $${total}</span>
        <button onclick="removeFromCart(${index})" style="background: none; border: none; color: #ff4d4d; font-size: 1.1rem; cursor: pointer;">❌</button>
      </li>
    `;
  });
  html += "</ul>";

  const total = cart.reduce((sum, item) => sum + (item.discountedPrice ?? item.price) * item.quantity, 0).toFixed(2);
  html += `<div id="cart-total">Total: <span style="color: #ffcc00;">$${total}</span></div>`;

  html += `
    <button onclick="openOrderModal()" style="
      margin-top: 1rem;
      background-color: #2db8ff;
      color: #121c26;
      font-weight: bold;
      border: none;
      border-radius: 6px;
      padding: 0.5rem 1rem;
      cursor: pointer;
      width: 100%;">Place Order</button>
  `;

  content.innerHTML = html;
}

function openOrderModal() {
  const modal = document.createElement("div");
  modal.className = "modal";
  modal.id = "orderModal";
  modal.style.display = "flex";

  modal.innerHTML = `
    <div class="modal-content">
      <span class="close" onclick="closeModal('orderModal')">&times;</span>
      <h2>Order Information</h2>
      <input type="text" id="orderName" placeholder="Full Name" />
      <input type="email" id="orderEmail" placeholder="Email" />
      <input type="text" id="orderAddress" placeholder="Delivery Address" />
      <button onclick="submitOrderFromModal()">Confirm Order</button>
    </div>
  `;

  document.body.appendChild(modal);
}

function closeModal(id) {
  const modal = document.getElementById(id);
  if (modal) modal.remove();
}

async function submitOrderFromModal() {
  const name = document.getElementById("orderName").value.trim();
  const email = document.getElementById("orderEmail").value.trim();
  const address = document.getElementById("orderAddress").value.trim();

  if (!name || !email || !address) {
    alert("Please fill in all order fields.");
    return;
  }

  const order = {
    customerName: name,
    customerEmail: email,
    deliveryAddress: address,
    total: cart.reduce((sum, item) => sum + item.price * item.quantity, 0),
    items: cart.map(item => ({
      productId: item.id,
      quantity: item.quantity
    })),
    placedAt: new Date().toISOString(),
    status: "Pending"
  };

  try {
    const response = await fetch(`${apiBaseUrl}/api/order`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(order)
    });

    const result = await response.json();

    if (result.success) {
      alert("Order placed successfully!");
      cart.length = 0;
      updateCartDisplay();
      updateCartCount();
      closeModal("orderModal");

      await saveCartToServer("demoUser");

    } else {
      alert("Order failed: " + result.message);
    }
  } catch (error) {
    console.error("Order error:", error);
    alert("Error placing order.");
  }
}

async function saveCartToServer(userId) {
  try {
    const items = cart.map(item => ({
      productId: item.id,
      quantity: item.quantity
    }));

    const response = await fetch(`${apiBaseUrl}/api/cart/save?userId=${userId}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(items)
    });

    if (!response.ok) throw new Error("Failed to save cart");
    console.log("Cart saved.");
  } catch (err) {
    console.error("Save cart error:", err);
  }
}

async function loadProducts() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/product`);
    if (!res.ok) throw new Error("Failed to load products");

    const products = await res.json();
    const container = document.getElementById("product-container");
    const grid = document.createElement("div");
    grid.className = "product-grid";

    products.forEach(product => {
      const card = document.createElement("div");
      card.className = "product-card";

      const hasDiscount = product.discountedPrice && product.discountedPrice < product.price;

      card.innerHTML = `
        <img src="${product.imageUrl}" alt="${product.name}" style="cursor: pointer;" onclick="openImageModal('${product.imageUrl}')">
        <h3>${product.name}</h3>
        <p>${product.description}</p>
        <div class="price">
          ${hasDiscount ? `<span style="text-decoration: line-through; color: gray;">$${product.price.toFixed(2)}</span> <span style="color: #ff4d4d; font-weight: bold; margin-left: 0.5rem;">$${product.discountedPrice.toFixed(2)}</span>` : `$${product.price.toFixed(2)}`}
        </div>
      `;

      const quantityWrapper = document.createElement("div");
      quantityWrapper.className = "quantity-wrapper";

      const minusBtn = document.createElement("button");
      minusBtn.textContent = "-";
      const plusBtn = document.createElement("button");
      plusBtn.textContent = "+";
      const quantityInput = document.createElement("input");
      quantityInput.type = "number";
      quantityInput.value = 1;
      quantityInput.min = 1;
      quantityInput.className = "quantity-input";

      minusBtn.onclick = () => {
        const val = parseInt(quantityInput.value);
        if (val > 1) quantityInput.value = val - 1;
      };

      plusBtn.onclick = () => {
        quantityInput.value = parseInt(quantityInput.value) + 1;
      };

      quantityWrapper.append(minusBtn, quantityInput, plusBtn);

      const addToCartBtn = document.createElement("button");
      addToCartBtn.className = "add-to-cart-btn";
      addToCartBtn.textContent = "Add to Cart";
      addToCartBtn.onclick = () => {
        const quantity = parseInt(quantityInput.value);
        addToCart(product, quantity);
      };

      card.appendChild(quantityWrapper);
      card.appendChild(addToCartBtn);
      grid.appendChild(card);
    });

    container.appendChild(grid);
  } catch (err) {
    console.error("Error loading products:", err);
  }
}

async function loadPasses() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/pass`);
    if (!res.ok) throw new Error("Failed to load passes");

    const passes = await res.json();
    const container = document.getElementById("pricing-container");
    container.innerHTML = "";

    passes.forEach(pass => {
      const card = document.createElement("div");
      card.className = "card";
      card.innerHTML = `
        <h3>${pass.name}</h3>
        <p>${pass.description}</p>
        <div class="price">$${pass.price.toFixed(2)}</div>
      `;
      container.appendChild(card);
    });
  } catch (err) {
    console.error("Error loading passes:", err);
  }
}

function openImageModal(src) {
  const modal = document.getElementById("imageModal");
  const modalImg = document.getElementById("modalImage");
  modalImg.src = src;
  modal.style.display = "flex";
}

function closeImageModal() {
  document.getElementById("imageModal").style.display = "none";
}

function openLoginModal() {
  const modal = document.getElementById("authModal");
  modal.style.display = "block";
  document.getElementById("loginForm").style.display = "block";
  document.getElementById("registerForm").style.display = "none";
}

function openRegisterModal() {
  const modal = document.getElementById("authModal");
  modal.style.display = "block";
  document.getElementById("registerForm").style.display = "block";
  document.getElementById("loginForm").style.display = "none";
}

function updatePhonePrefix() {
  const select = document.getElementById("countrySelect");
  const prefix = select.value;
  const phoneInput = document.getElementById("phoneInput");
  if (!phoneInput.value.startsWith(prefix)) {
    phoneInput.value = prefix + " ";
  }
}

async function handleRegister() {
  const name = document.getElementById("registerName").value.trim();
  const email = document.getElementById("registerEmail").value.trim();
  const confirmEmail = document.getElementById("confirmEmail").value.trim();
  const password = document.getElementById("registerPassword").value;
  const confirmPassword = document.getElementById("confirmPassword").value;
  const dob = document.getElementById("dob").value;
  const address = document.getElementById("address").value.trim();
  const phone = document.getElementById("phoneInput").value.trim();

  if (!name || !email || !password || !confirmEmail || !confirmPassword || !dob || !address || !phone) {
    alert("Please fill in all fields.");
    return;
  }

  if (email !== confirmEmail) {
    alert("Emails do not match.");
    return;
  }

  if (password !== confirmPassword) {
    alert("Passwords do not match.");
    return;
  }

  const user = { name, email, password, dob, address, phone };

  try {
    const response = await fetch(`${apiBaseUrl}/api/user/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(user)
    });

    const result = await response.json();
    if (result.success) {
      alert("Registration successful!");
      closeModal("authModal");
    } else {
      alert("Registration failed: " + result.message);
    }
  } catch (err) {
    console.error("Registration error:", err);
    alert("Error during registration.");
  }
}

function handleLogin() {
  alert("Login logic to be implemented.");
}

function openServiceModal(serviceType) {
  const modal = document.getElementById("serviceModal");
  const content = document.getElementById("serviceDetails");

  let title = "";
  let description = "";

  switch (serviceType) {
    case "group":
      title = "Group Training";
      description = "Join our high-energy group training sessions led by certified trainers.";
      break;
    case "coaching":
      title = "Personal Coaching";
      description = "Get 1-on-1 coaching tailored to your fitness goals.";
      break;
    case "nutrition":
      title = "Nutrition Plans";
      description = "Personalized nutrition guidance from experts.";
      break;
    default:
      title = "Service";
      description = "Details coming soon.";
  }

  content.innerHTML = `<h2>${title}</h2><p>${description}</p>`;
  modal.style.display = "block";
}

window.addEventListener("DOMContentLoaded", () => {
  loadProducts();
  loadPasses();

  document.getElementById("loginBtn")?.addEventListener("click", openLoginModal);
  document.getElementById("registerBtn")?.addEventListener("click", openRegisterModal);
  document.querySelectorAll(".close").forEach(btn => {
    btn.addEventListener("click", () => closeModal(btn.closest(".modal").id));
  });

  window.addEventListener("click", (event) => {
    const modals = ["authModal", "serviceModal", "orderModal"];
    modals.forEach(id => {
      const modal = document.getElementById(id);
      if (event.target === modal) {
        modal.style.display = "none";
      }
    });
  });

  document.getElementById("countrySelect")?.addEventListener("change", updatePhonePrefix);

  const cartIcon = document.querySelector(".cart-icon");
  const cartContainer = document.getElementById("cart-container");

  if (cartIcon && cartContainer) {
    cartIcon.addEventListener("click", (e) => {
      e.stopPropagation();
      const isVisible = cartContainer.style.display === "block";
      cartContainer.style.display = isVisible ? "none" : "block";
    });

    document.addEventListener("click", (e) => {
      if (!cartContainer.contains(e.target) && !cartIcon.contains(e.target)) {
        cartContainer.style.display = "none";
      }
    });
  }
});