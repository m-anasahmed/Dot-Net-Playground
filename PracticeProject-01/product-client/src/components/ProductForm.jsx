import React, {useEffect, useState} from "react";
import api from "../api";

export default function ProductForm({product, onSaved}) {
  const [name,setName] = useState("");
  const [price,setPrice] = useState("");
  const [loading,setLoading] = useState(false);
  const [error,setError] = useState(null);

  useEffect(()=>{
    if(product){ setName(product.name); setPrice(product.price); }
    else { setName(""); setPrice(""); }
  },[product]);

  const submit = async (e) => {
    e.preventDefault();
    setLoading(true); setError(null);
    try {
      const body = { id: product?.id, name, price: parseFloat(price) || 0 };
      if(product?.id){
        await api.put(`/products/${product.id}`, body);
      } else {
        await api.post("/products", body);
      }
      onSaved?.();
    } catch (err) {
      setError(err.response?.data || err.message);
    } finally { setLoading(false); }
  };

  return (
    <form onSubmit={submit} className="p-4 border rounded">
      <div>
        <label>Name</label>
        <input value={name} onChange={e=>setName(e.target.value)} required />
      </div>
      <div>
        <label>Price</label>
        <input value={price} onChange={e=>setPrice(e.target.value)} required type="number" step="0.01" />
      </div>
      <div>
        <button disabled={loading} type="submit">{product?.id ? "Update" : "Add"}</button>
      </div>
      {error && <div className="text-red-600">{String(error)}</div>}
    </form>
  );
}
